using BlogProject.Core.CustomException;
using BlogProject.Core.Models.ViewModels;
using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Data.Methods;

public class AdministratorMethods(ApplicationDbContext context, string? currentUserId, UserManager<User> userManager)
    : IMethods
{
    public async Task<(List<ArticleViewModel>, bool)> GetAllArticles(int page, int pageSize = 10)
    {
        var allArticles = context.Articles
            .Include(a => a.User)
            .Include(a => a.Comments)!
            .ThenInclude(c => c.User)
            .Include(a => a.Tags) // Добавляем загрузку тегов
            .OrderByDescending(a => a.CreatedDate);

        // Получаем общее количество для пагинации
        var totalCount = await allArticles.CountAsync();

        // Получаем данные страницы
        var articles = await allArticles
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticleViewModel
            {
                ArticleId = a.Id,
                Title = a.Title,
                Content = a.Content,
                AuthorFullName = a.User != null
                    ? $"{a.User.FirstName} {a.User.LastName}"
                    : "Аноним",
                CreatedDate = a.CreatedDate,
                UpdatedDate = a.UpdatedDate,
                UserId = a.UserId,
                Deletable = true,
                Editable = true,
                Tag = a.Tags != null
                    ? a.Tags.Select(t => new TagViewModel { Text = t.Name }).ToList()
                    : new List<TagViewModel>(),
                //Tag = a.Tags.Select(t => new TagViewModel { Text = t.Name }).ToList(), // Проекция тегов
                Comments = a.Comments != null
                    ? a.Comments.Select(c => new CommentViewModel
                    {
                        CommentId = c.Id,
                        Text = c.Text,
                        Author = c.User != null
                            ? $"{c.User.FirstName} {c.User.LastName}"
                            : "Аноним",
                        CreatedDate = c.CreatedDate,
                        UpdatedDate = c.UpdatedDate,
                        Deletable = true,
                        Editable = true
                    }).ToList()
                    : new List<CommentViewModel>()
            }).ToListAsync();

        var hasMore = totalCount > page * pageSize;

        return (articles, hasMore);
    }

    public async Task<(List<ArticleViewModel>, bool)> GetAllArticlesByTag(List<string> tags, int page, int pageSize = 10)
    {
        // Нормализация тегов: обрезка пробелов и приведение к верхнему регистру
        var normalizedTags = tags
            .Select(t => t.Trim().ToUpper())
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct()
            .ToList();


        if (normalizedTags.Count == 0)
        {
            return ([], false);
        }


        var allArticles = context.Articles
            .Include(a => a.User)
            .Include(a => a.Comments)!
                .ThenInclude(c => c.User)
            .Include(a => a.Tags)
            .Where(a => a.Tags != null && a.Tags.Any(t => normalizedTags.Contains(t.Name.ToUpper())))
            .OrderByDescending(a => a.CreatedDate);

        // Получаем общее количество
        var totalCount = await allArticles.CountAsync();

        // Применяем пагинацию
        var articles = await allArticles
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticleViewModel
            {
                ArticleId = a.Id,
                Title = a.Title,
                Content = a.Content,
                AuthorFullName = a.User != null
                    ? $"{a.User.FirstName} {a.User.LastName}"
                    : "Аноним",
                CreatedDate = a.CreatedDate,
                UpdatedDate = a.UpdatedDate,
                UserId = a.UserId,
                Deletable = true,
                Editable = true,
                Tag = a.Tags != null ? a.Tags.Select(t => new TagViewModel { Text = t.Name }).ToList() : new List<TagViewModel>(),
                Comments = a.Comments != null
                    ? a.Comments.Select(c => new CommentViewModel
                    {
                        CommentId = c.Id,
                        Text = c.Text,
                        Author = c.User != null
                            ? $"{c.User.FirstName} {c.User.LastName}"
                            : "Аноним",
                        CreatedDate = c.CreatedDate,
                        UpdatedDate = c.UpdatedDate,
                        Deletable = true,
                        Editable = true
                    }).ToList()
                    : new List<CommentViewModel>()
            }).ToListAsync();

        var hasMore = totalCount > page * pageSize;

        return (articles, hasMore);
    }

    public async Task<(List<ArticleViewModel>, bool)> GetArticlesByUserId(string? userId, int page, int pageSize = 10)
    {
        // Проверка UserID
        var targetUserId = userId ?? currentUserId;

        // Получаем все статьи из контекста базы данных
        var allArticles = await context.Articles
            .Where(m => m.UserId == targetUserId)
            .Include(m => m.Comments)!
            .ThenInclude(c => c.User)
            .Include(a => a.Tags)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();

        // Постраничная разбивка
        var articles = allArticles
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        // Собираем все уникальные Id авторов из статей и комментариев
        var authorsIds = articles
            .Select(m => m.UserId)
            .Concat(articles.SelectMany(m => m.Comments!).Select(c => c.UserId))
            .Where(id => id != null)
            .Distinct()
            .ToList();

        // Загружаем только нужных пользователей
        Dictionary<string, User> users = await userManager.Users
            .Where(u => authorsIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id!, u => u);

        // Формируем ViewModel
        var result = articles.Select(a => new ArticleViewModel
        {
            ArticleId = a.Id,
            Title = a.Title,
            Content = a.Content,
            // AuthorFullName = users.ContainsKey(m.UserId!) ? users[m.UserId!].GetFullName() : "Аноним",
            AuthorFullName = users.TryGetValue(a.UserId!, out var author) ? author.GetFullName() : "Аноним",
            CreatedDate = a.CreatedDate,
            UpdatedDate = a.UpdatedDate,
            Deletable = true,
            Editable = true,
            Tag = a.Tags != null ? a.Tags.Select(t => new TagViewModel { Text = t.Name }).ToList() : [],
            Comments = a.Comments != null
                ? a.Comments.Select(c => new CommentViewModel
                {
                    CommentId = c.Id,
                    Text = c.Text,
                    Author = users.TryGetValue(c.UserId!, out var commentAuthor) ? commentAuthor.GetFullName() : "Аноним",
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    Deletable = true,
                    Editable = true,
                }).ToList()
                : []
        }).ToList();

        var hasMore = (allArticles.Count / pageSize - page) >= 0;

        return (result, hasMore);
    }

    public async Task<ArticleViewModel?> GetArticleById(int articleId)
    {
        var article = await context.Articles
            .Include(a => a.User)
            .Include(a => a.Comments)!
            .ThenInclude(c => c.User)
            .Include(a => a.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == articleId);

        return article == null
            ? throw new NotFoundException("Статья не найдена")
            : new ArticleViewModel
            {
                ArticleId = article.Id,
                Title = article.Title,
                Content = article.Content,
                AuthorFullName = article.User != null
                    ? $"{article.User.FirstName} {article.User.LastName}"
                    : "Аноним",
                CreatedDate = article.CreatedDate,
                UpdatedDate = article.UpdatedDate,
                UserId = article.UserId,
                Deletable = true,
                Editable = true,
                Tag = article.Tags != null ? article.Tags.Select(t => new TagViewModel { Text = t.Name }).ToList() : [],
                Comments = article.Comments != null
                    ? article.Comments.Select(c => new CommentViewModel
                    {
                        CommentId = c.Id,
                        Text = c.Text,
                        Author = c.User != null
                            ? $"{c.User.FirstName} {c.User.LastName}"
                            : "Аноним",
                        CreatedDate = c.CreatedDate,
                        UpdatedDate = c.UpdatedDate,
                        Deletable = true,
                        Editable = true
                    }).ToList()
                    : []
            };
    }

    public async Task CreateArticle(ArticleViewModel model)
    {
        if (model == null)
            throw new AppException("Неверные данные статьи", 400);

        if (currentUserId == null) throw new NotFoundException("Пользователь не найден при создании статьи");

        var article = new Article
        {
            Title = model.Title,
            Content = model.Content!,
            UserId = currentUserId,
            CreatedDate = DateTime.UtcNow,
            Tags = []
        };

        // Извлекаем теги из модели, фильтруем пустые и дубликаты
        var tagNames = model.Tag?
            .Select(t => t.Text?.Trim())
            .Where(text => !string.IsNullOrEmpty(text))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        foreach (var tagName in tagNames)
        {
            var normalizedTagName = tagName!.ToUpper();

            var existingTag = await context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToUpper() == normalizedTagName);

            if (existingTag != null)
            {
                article.Tags?.Add(existingTag);
            }
            else
            {
                var newTag = new Tag
                {
                    Name = normalizedTagName
                };

                context.Tags.Add(newTag);
                article.Tags?.Add(newTag);
            }
        }

        context.Articles.Add(article);
        await context.SaveChangesAsync();
    }

    public async Task EditArticle(int articleId, ArticleViewModel model)
    {
        if (model == null)
            throw new AppException("Неверные данные статьи", 400);

        var article = await context.Articles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == articleId) ?? throw new ForbiddenException("Статья не найдена");

        // Обновление основных полей
        article.Title = model.Title;
        article.Content = model.Content;
        article.UpdatedDate = DateTime.UtcNow;

        // Очистка текущих тегов
        article.Tags?.Clear();

        // Обработка новых тегов из модели
        var tagNames = model.Tag?
            .Select(t => t.Text?.Trim())
            .Where(text => !string.IsNullOrEmpty(text))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        foreach (var tagName in tagNames)
        {

            if (tagName == null) continue;
            var normalizedTagName = tagName.ToUpper();

            var existingTag = await context.Tags
                .FirstOrDefaultAsync(t => t.Name.Equals(normalizedTagName, StringComparison.CurrentCultureIgnoreCase));

            if (existingTag != null)
            {
                article.Tags?.Add(existingTag);
            }
            else
            {
                var newTag = new Tag
                {
                    Name = normalizedTagName.Trim() // Сохраняем с обрезанными пробелами
                };

                context.Tags.Add(newTag); // Добавление нового тега в контекст
                article.Tags?.Add(newTag);
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteArticle(int articleId)
    {
        var article = await context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId) ?? throw new ForbiddenException("Статья не найдена");
        context.Articles.Remove(article);
        await context.SaveChangesAsync();
    }

    public async Task CreateComment(int articleId, CommentViewModel model)
    {
        if (model == null)
            throw new AppException("Неверные данные комментария", 400);

        if (string.IsNullOrWhiteSpace(model.Text))
            throw new AppException("Текст комментария не может быть пустым", 400);

        var comment = new Comment
        {
            Text = model.Text.Trim(),
            UserId = currentUserId,
            ArticleId = articleId,
            CreatedDate = DateTime.UtcNow
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync();
    }

    public async Task EditComment(int commentId, CommentViewModel model)
    {
        if (model == null)
            throw new AppException("Неверные данные комментария", 400);

        if (string.IsNullOrWhiteSpace(model.Text))
            throw new AppException("Текст комментария не может быть пустым", 400);

        var comment = await context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId) ?? throw new ForbiddenException("Комментарий не найден");
        comment.Text = model.Text.Trim(); // Удаляем начальные/конечные пробелы

        comment.UpdatedDate = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task DeleteComment(int commentId)
    {
        var comment = await context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId) ?? throw new ForbiddenException("Комментарий не найден");
        context.Comments.Remove(comment);
        await context.SaveChangesAsync();
    }

    public async Task<(List<UserViewModel>, bool)> GetAllUsersAsync(int page, int pageSize = 10)
    {
        var allUsers = context.Users
            .Where(u => u.Id != currentUserId) // Исключаем текущего пользователя
            .Include(u => u.Articles) // Подключаем статьи для подсчета
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName);

        // Получаем общее количество пользователей
        var totalCount = await allUsers.CountAsync();

        // Получаем данные страницы
        var users = await allUsers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserViewModel
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                ArticleCount = u.Articles != null ? u.Articles.Count : 0, // Проверка на null
                Deletable = true
            })
            .ToListAsync();

        var hasMore = totalCount > page * pageSize;

        return (users, hasMore);
    }

    public async Task<UserViewModel> GetUserInfoAsync(string? userId = null)
    {
        userId ??= currentUserId ?? throw new NotFoundException("Пользователь не найден");
        var deletable = userId == currentUserId;

        var user = await context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.Articles)
            .Select(u => new UserViewModel
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                ArticleCount = u.Articles != null ? u.Articles.Count : 0,
                Deletable = deletable
            })
            .FirstOrDefaultAsync() ?? throw new NotFoundException("Пользователь не найден");

        return user;
    }
    public async Task<IdentityResult> EditUserProfile(string userId, UpdateUserViewModel profile)
    {
        var user = await userManager.FindByIdAsync(userId) ?? throw new NotFoundException("Пользователь не найден");

        // Обновление основных полей
        user.FirstName = profile.FirstName;
        user.LastName = profile.LastName;

        // Обновление пароля при необходимости
        if (!string.IsNullOrEmpty(profile.NewPassword))
        {
            // Проверка текущего пароля
            var passwordCheck = await userManager.CheckPasswordAsync(user, profile.CurrentPassword);
            if (!passwordCheck)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Неверный текущий пароль"
                });
            }

            // Смена пароля
            var changeResult = await userManager.ChangePasswordAsync(
                user,
                profile.CurrentPassword,
                profile.NewPassword);

            if (!changeResult.Succeeded)
            {
                return changeResult;
            }
        }

        // Сохранение изменений профиля
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return updateResult;
        }

        return IdentityResult.Success;
    }

    public async Task DeleteUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId) ?? throw new NotFoundException("Пользователь не найден");

        await userManager.DeleteAsync(user);
    }
}