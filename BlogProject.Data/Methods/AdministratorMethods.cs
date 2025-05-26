using BlogProject.Core.Models.ViewModels;
using BlogProject.Core.Models.ViewModels.DTO;
using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Data.Methods;

public class AdministratorMethods : IMethods
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly string _currentUserId;

    public AdministratorMethods(ApplicationDbContext context, string currentUserId, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
        _currentUserId = currentUserId;
    }

    public async Task<(List<ArticleViewModel>, bool)> GetUserArticles(string? userId, int page, int pageSize = 10)
    {
        // Проверка UserID
        var targetUserId = userId ?? _currentUserId;

        // Получаем все статьи из контекста базы данных
        var allArticles = await _context.Articles
            .Where(m => m.UserId == targetUserId)
            .Include(m => m.Comments)
            .ThenInclude(c => c.User)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();

        // Постраничная разбивка
        var articles = allArticles
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        // Собираем все уникальные Id авторов из статей и комментариев
        var authorsIds = articles
            .Select(m => m.UserId)
            .Concat(articles.SelectMany(m => m.Comments).Select(c => c.UserId))
            .Where(id => id != null)
            .Distinct()
            .ToList();

        // Загружаем только нужных пользователей
        var users = await _userManager.Users
            .Where(u => authorsIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u);


        // Формируем ViewModel
        var result = articles.Select(m => new ArticleViewModel
        {
            ArticleId = m.Id,
            Title = m.Title,
            Content = m.Content,
            AuthorFullName = users.ContainsKey(m.UserId!) ? users[m.UserId!].GetFullName() : "Аноним",
            CreatedDate = m.CreatedDate,
            UpdatedDate = m.UpdatedDate,
            Deletable = true,
            Comments = m.Comments.Select(c => new CommentViewModel
            {
                CommentId = c.Id,
                Text = c.Text,
                Author = users.ContainsKey(c.UserId!) ? users[c.UserId!].GetFullName() : "Аноним",
                CreatedDate = c.CreatedDate,
                UpdatedDate = c.UpdatedDate,
                Deletable = true,
            }).ToList()
        }).ToList();

        var hasMore = (allArticles.Count / pageSize - page) >= 0;

        return (result, hasMore);
    }

    public async Task<(List<ArticleViewModel>, bool)> GetAllArticles(int page, int pageSize = 10)
    {

        var allArticles = _context.Articles
            .Include(a => a.User)
            .Include(a => a.Comments)
            .ThenInclude(c => c.User)
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
                Comments = a.Comments.Select(c => new CommentViewModel
                {
                    CommentId = c.Id,
                    Text = c.Text,
                    Author = c.User != null
                        ? $"{c.User.FirstName} {c.User.LastName}"
                        : "Аноним",
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    Deletable = true
                }).ToList()
            }).ToListAsync();

        var hasMore = totalCount > page * pageSize;

        return (articles, hasMore);
    }

    public async Task CreateArticle(string title, string content, List<string> tags)
    {
        var article = new Article
        {
            Title = title,
            Content = content,
            UserId = _currentUserId,
            CreatedDate = DateTime.UtcNow,
            Tags = new List<Tag>()
        };

        // Обрабатываем теги
        foreach (var tagName in tags.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var normalizedTagName = tagName.Trim().ToUpper();

            // Ищем существующий тег
            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToUpper() == normalizedTagName);

            if (existingTag != null)
            {
                // Используем существующий тег
                article.Tags.Add(existingTag);
            }
            else
            {
                // Создаем новый тег
                var newTag = new Tag
                {
                    Name = tagName.Trim(),
                };

                _context.Tags.Add(newTag);
                article.Tags.Add(newTag);
            }
        }

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
    }

    public async Task EditArticle(int articleId, string title, string content, List<string> tags)
    {
        var article = await _context.Articles
            .Include(a => a.Tags) // Включаем теги для работы со связями
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null) return;

        // Обновляем основные поля
        article.Title = title;
        article.Content = content;
        article.UpdatedDate = DateTime.UtcNow;

        // Удаляем все текущие теги
        article.Tags.Clear();

        // Добавляем новые теги
        foreach (var tagName in tags.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var normalizedTagName = tagName.Trim().ToUpperInvariant();

            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToUpper() == normalizedTagName);

            if (existingTag != null)
            {
                article.Tags.Add(existingTag);
            }
            else
            {
                article.Tags.Add(new Tag
                {
                    Name = tagName.Trim(),
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteArticle(int articleId)
    {
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null) return;

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();
    }

    public async Task CreateComment(int articleId, string text)
    {
        var comment = new Comment
        {
            Text = text,
            UserId = _currentUserId,
            ArticleId = articleId,
            CreatedDate = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
    }

    public async Task EditComment(int commentId, string text)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null) return;

        comment.Text = text;
        comment.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteComment(int commentId)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null) return;

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<UserViewModel>, bool)> GetAllUsersAsync(int page, int pageSize = 10)
    {
        var allUsers = _context.Users
            .Where(u => u.Id != _currentUserId) // Исключаем текущего пользователя
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
                ArticleCount = u.Articles.Count,
                Deletable = true
            })
            .ToListAsync();

        var hasMore = totalCount > page * pageSize;

        return (users, hasMore);
    }

    public async Task<IdentityResult> EditUserProfile(string userId, UpdateUserViewModel profile)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "Пользователь не найден"
            });
        }

        // Обновление основных полей
        user.FirstName = profile.FirstName;
        user.LastName = profile.LastName;

        // Обновление пароля при необходимости
        if (!string.IsNullOrEmpty(profile.NewPassword))
        {
            // Проверка текущего пароля
            var passwordCheck = await _userManager.CheckPasswordAsync(user, profile.CurrentPassword);
            if (!passwordCheck)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Неверный текущий пароль"
                });
            }

            // Смена пароля
            var changeResult = await _userManager.ChangePasswordAsync(
                user,
                profile.CurrentPassword,
                profile.NewPassword);

            if (!changeResult.Succeeded)
            {
                return changeResult;
            }
        }

        // Сохранение изменений профиля
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return updateResult;
        }

        return IdentityResult.Success;
    }

    public async Task DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return;

        await _userManager.DeleteAsync(user);
    }

}