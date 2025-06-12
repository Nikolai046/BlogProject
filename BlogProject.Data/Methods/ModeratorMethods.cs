using BlogProject.Core.CustomException;
using BlogProject.Core.Models.ViewModels;
using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Data.Methods;

public class ModeratorMethods(ApplicationDbContext context, string? currentUserId, UserManager<User> userManager)
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

        if (totalCount == 0) return ([], false);

        // Если запрошена страница превышающая общее количество страниц, устанавливаем её на последнюю
        var lastPage = (int)Math.Ceiling((double)totalCount / pageSize);
        page = Math.Clamp(page, 1, lastPage);

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
                Deletable = a.UserId == currentUserId,
                Editable = true,
                Tag = a.Tags != null ? a.Tags.Select(t => new TagViewModel { Text = t.Name }).ToList() : new List<TagViewModel>(), // Проекция тегов
                Comments = a.Comments != null ? a.Comments.Select(c => new CommentViewModel
                {
                    CommentId = c.Id,
                    Text = c.Text,
                    Author = c.User != null
                        ? $"{c.User.FirstName} {c.User.LastName}"
                        : "Аноним",
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    Deletable = c.UserId == currentUserId,
                    Editable = true,
                }).ToList() : new List<CommentViewModel>()
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
            .Where(a => a.Tags!.Any(t => normalizedTags.Contains(t.Name.ToUpper())))
            .OrderByDescending(a => a.CreatedDate)
            .AsQueryable();

        // Получаем общее количество
        var totalCount = await allArticles.CountAsync();

        if (totalCount == 0) return ([], false);

        // Если запрошена страница превышающая общее количество страниц, устанавливаем её на последнюю
        var lastPage = (int)Math.Ceiling((double)totalCount / pageSize);
        page = Math.Clamp(page, 1, lastPage);

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
                Deletable = a.UserId == currentUserId,
                Editable = true,
                Tag = a.Tags != null ? a.Tags.Select(t => new TagViewModel { Text = t.Name }).ToList() : new List<TagViewModel>(),
                Comments = a.Comments != null ? a.Comments.Select(c => new CommentViewModel
                {
                    CommentId = c.Id,
                    Text = c.Text,
                    Author = c.User != null
                        ? $"{c.User.FirstName} {c.User.LastName}"
                        : "Аноним",
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    Deletable = c.UserId == currentUserId,
                    Editable = true,
                }).ToList() : new List<CommentViewModel>()
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

        // Получаем общее количество
        var totalCount = allArticles.Count();

        if (totalCount == 0) return ([], false);

        // Если запрошена страница превышающая общее количество страниц, устанавливаем её на последнюю
        var lastPage = (int)Math.Ceiling((double)totalCount / pageSize);
        page = Math.Clamp(page, 1, lastPage);

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
        var users = await userManager.Users
            .Where(u => authorsIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id!, u => u);

        // Формируем ViewModel
        var result = articles.Select(a => new ArticleViewModel
        {
            ArticleId = a.Id,
            Title = a.Title,
            Content = a.Content,
            AuthorFullName = users.TryGetValue(a.UserId!, out var author) ? author.GetFullName() : "Аноним",
            CreatedDate = a.CreatedDate,
            UpdatedDate = a.UpdatedDate,
            Deletable = (a.UserId == currentUserId!),
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
                    Deletable = (c.UserId == currentUserId),
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
                Deletable = article.UserId == currentUserId,
                Editable = true,
                Tag = article.Tags != null
                    ? article.Tags.Select(t => new TagViewModel { Text = t.Name }).ToList()
                    : [],
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
                        Deletable = c.UserId == currentUserId,
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
            CreatedDate = DateTime.Now,
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
        article.UpdatedDate = DateTime.Now;

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
                .FirstOrDefaultAsync(t => t.Name.ToUpper() == normalizedTagName);

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
            .FirstOrDefaultAsync(a => a.Id == articleId && a.UserId == currentUserId) ?? throw new ForbiddenException("Статья не найдена или у вас нет прав на удаление");
        context.Articles.Remove(article);
        await context.SaveChangesAsync();
    }

    public async Task CreateComment(int articleId, CommentViewModel model)
    {
        if (model == null)
            throw new AppException("Неверные данные комментария", 400);

        if (string.IsNullOrWhiteSpace(model.Text!.Trim()))
            throw new AppException("Текст комментария не может быть пустым", 400);

        var comment = new Comment
        {
            Text = model.Text.Trim(),
            UserId = currentUserId,
            ArticleId = articleId,
            CreatedDate = DateTime.Now
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync();
    }

    public async Task EditComment(int commentId, CommentViewModel model)
    {
        if (model == null)
            throw new AppException("Неверные данные комментария", 400);

        if (string.IsNullOrWhiteSpace(model.Text!.Trim()))
            throw new AppException("Текст комментария не может быть пустым", 400);

        var comment = await context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId) ?? throw new ForbiddenException("Комментарий не найден");

        comment.Text = model.Text.Trim(); // Удаляем начальные/конечные пробелы

        comment.UpdatedDate = DateTime.Now;

        await context.SaveChangesAsync();
    }

    public async Task DeleteComment(int commentId)
    {
        var comment = await context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == currentUserId) ?? throw new ForbiddenException("Комментарий не найден или у вас нет прав на удаление");

        context.Comments.Remove(comment);

        await context.SaveChangesAsync();
    }

    public async Task<(List<UserViewModel>, bool)> GetAllUsersAsync(int page, int pageSize = 10)
    {
        var allUsers = context.Users
            .Where(u => u.Id != currentUserId)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Include(u => u.Articles!)
                .ThenInclude(a => a.Tags);

        var userRolesQuery = context.UserRoles
            .Join(context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new { ur.UserId, RoleName = r.Name });

        var totalCount = await allUsers.CountAsync();

        // Защита от page < 1 и пустой БД
        var lastPage = totalCount > 0
            ? (int)Math.Ceiling((double)totalCount / pageSize)
            : 1;
        page = Math.Clamp(page, 1, lastPage);
        List<UserViewModel> users;

        try
        {
            // Получаем данные пользователей с загруженными статьями и тегами
            var usersWithData = await allUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Преобразуем в ViewModel и обрабатываем теги в памяти
            users = usersWithData.Select(u => {
                // Собираем все уникальные теги из всех статей пользователя
                var allTags = u.Articles!
                    .SelectMany(a => a.Tags!)
                    .Select(t => t.Name)
                    .Distinct()
                    .OrderBy(name => name)
                    .Select(name => new TagViewModel { Text = name })
                    .ToList();

                return new UserViewModel
                {
                    UserId = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Deletable = false,
                    ArticleCount = u.Articles!.Count,
                    Tag = allTags,
                    Roles = userRolesQuery
                        .Where(ur => ur.UserId == u.Id)
                        .Select(ur => ur.RoleName)
                        .ToList()!
                };
            }).ToList();
        }
        catch (Exception ex)
        {
            // Логируйте полную ошибку, включая InnerException
            Console.WriteLine($"Ошибка: {ex.Message}\n{ex.InnerException?.Message}");
            throw;
        }

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
        if (userId != currentUserId)
            throw new ForbiddenException("Редактирование профиля запрещено");

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
        if (userId != currentUserId)
            throw new ForbiddenException("Удаление пользователя запрещено");

        var user = await userManager.FindByIdAsync(userId) ?? throw new NotFoundException("Пользователь не найден");

        await userManager.DeleteAsync(user);
    }

    public async Task<string?> FindUserIdsByNameAsync(string name)
    {
        name = name.ToUpper();
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length switch
        {
            0 => null,
            1 => await userManager.Users
                .Where(u => u.FirstName!.ToUpper() == words[0] || u.LastName!.ToUpper() == words[0])
                .Select(u => u.Id)
                .FirstOrDefaultAsync(),
            _ => await userManager.Users
                .Where(u => (u.FirstName!.ToUpper() == words[0] && u.LastName!.ToUpper() == words[1]) ||
                            (u.FirstName.ToUpper() == words[1] && u.LastName!.ToUpper() == words[0]))
                .Select(u => u.Id)
                .FirstOrDefaultAsync()
        };
    }
}