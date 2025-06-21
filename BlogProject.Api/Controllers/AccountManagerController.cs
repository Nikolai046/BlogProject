using BlogProject.Api.Services;
using BlogProject.Core.Models.RequestModels;
using BlogProject.Core.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace BlogProject.Api.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class AccountManagerController(GetUserPermissions permissions) : ControllerBase
{
    [HttpGet("get_user-info")]
    public async Task<IActionResult> GetUserInfo(string? userId)
    {
        var methods = await permissions.GetMethodsAsync();
        try
        {
            var (user, roleList) = await methods.GetUserInfoAsync(userId);
            var model = new UserInfoResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                ArticleCount = user.ArticleCount,
                Roles = user.Roles.Where(role => role != null).Select(role => role!).ToList()
            };
            return Ok(model);
        }
        catch
        {
            return NotFound(new { Message = $"Пользователь не найден c Id: {userId}" });
        }
    }

    [HttpGet("get_all-users")]
    public async Task<IActionResult> GetAllUsers(int page = 1, int pageSize = 10)
    {
        var methods = await permissions.GetMethodsAsync();
        try
        {
            var (users, hasMore, lastPage) = await methods.GetAllUsersAsync(page, pageSize);

            var response = new UserListRequest
            {
                Users = users,
                HasMore = hasMore,
                Page = page,
                PageSize = pageSize,
                TotalPages = lastPage
            };

            return Ok(response);
        }
        catch
        {
            return NotFound(new { Message = "Зарегистрированные пользователи отсутствуют" });
        }
    }

    [HttpGet("get_all-articles")]
    public async Task<IActionResult> GetAllArticles(int page = 1, int pageSize = 10)
    {
        var methods = await permissions.GetMethodsAsync();

        var (articles, hasMore, lastPage) = await methods.GetAllArticlesAsync(page, pageSize);

        var response = new ArticlesResponse
        {
            Articles = articles,
            HasMore = hasMore,
            Page = page,
            PageSize = pageSize,
            TotalPages = lastPage
        };

        return Ok(response);
    }

    [HttpGet("get_articles-by-author-id")]
    public async Task<IActionResult> ArticlesByAuthorId(string userId, int page = 1, int pageSize = 10)
    {
        var methods = await permissions.GetMethodsAsync();

        var (articles, hasMore, lastPage) = await methods.GetArticlesByUserIdAsync(userId, page, pageSize);

        if (!articles.Any()) return NotFound(new { Message = $"У пользователя Id: {userId} статей нет" });

        var response = new ArticlesResponse
        {
            Articles = articles,
            HasMore = hasMore,
            Page = page,
            PageSize = pageSize,
            TotalPages = lastPage
        };

        return Ok(response);
    }

    [HttpGet("get_articles-by-username")]
    public async Task<IActionResult> ArticlesByUsername(string userName, int page = 1, int pageSize = 10)
    {
        var methods = await permissions.GetMethodsAsync();

        var userId = await methods.FindUserIdsByNameAsync(userName);

        if (userId == null) return NotFound(new { Message = "Пользователь не найден" });

        var (articles, hasMore, lastPage) = await methods.GetArticlesByUserIdAsync(userId, page, pageSize);

        if (!articles.Any()) return NotFound(new { Message = $"У пользователя Id: {userId} статей нет" });

        var response = new ArticlesResponse
        {
            Articles = articles,
            HasMore = hasMore,
            Page = page,
            PageSize = pageSize,
            TotalPages = lastPage
        };

        return Ok(response);
    }

    [HttpGet("get_articles-by-tags")]
    public async Task<IActionResult> ArticlesByTags(string tagList, int page = 1, int pageSize = 10)
    {
        var tags = Regex.Split(tagList, @"\W+").Where(word => !string.IsNullOrEmpty(word)).ToList();

        var methods = await permissions.GetMethodsAsync();

        var (articles, hasMore, lastPage) = await methods.GetAllArticlesByTagAsync(tags, page, pageSize);

        if (!articles.Any()) return NotFound(new { Message = $"Нет статей со следующими тегами: {tagList}" });

        var response = new ArticlesResponse
        {
            Articles = articles,
            HasMore = hasMore,
            Page = page,
            PageSize = pageSize,
            TotalPages = lastPage
        };

        return Ok(response);
    }

    [HttpPost("create-article")]
    public async Task<IActionResult> CreateArticle(ArticleCreateResponse response)
    {
        var tags = response.Tags != null ? Regex.Split(response.Tags, @"\W+").Where(word => !string.IsNullOrEmpty(word)).ToList() : new List<string>();

        var model = new ArticleViewModel()
        {
            Title = response.Title,
            Content = response.Content,
            Tag = tags.Select(tag => new TagViewModel { Text = tag }).ToList(),
        };

        var methods = await permissions.GetMethodsAsync();
        await methods.CreateArticleAsync(model);

        return Ok(new { Message = "Статья успешно создана" });
    }

    [HttpPost("edit-article")]
    public async Task<IActionResult> EditArticle(ArticleEditResponse response)
    {
        var tags = response.Tags != null ? Regex.Split(response.Tags, @"\W+").Where(word => !string.IsNullOrEmpty(word)).ToList() : new List<string>();

        var methods = await permissions.GetMethodsAsync();

        var model = new ArticleViewModel()
        {
            Title = response.Title,
            Content = response.Content,
            Tag = tags.Select(tag => new TagViewModel { Text = tag }).ToList(),
        };

        await methods.EditArticleAsync(response.ArticleId, model);

        return Ok(new { Message = $"Статья Id: {response.ArticleId} обновлена" });
    }

    [HttpPost("create-comment")]
    public async Task<IActionResult> CreateComment(CommentCreateResponse response)
    {
        var comment = new CommentViewModel
        {
            Text = response.Content
        };
        var methods = await permissions.GetMethodsAsync();

        await methods.CreateCommentAsync(response.ArticleId, comment);

        return Ok(new { Message = $"Комментарий к статье Id: {response.ArticleId} добавлен" });
    }

    [HttpPost("edit-comment")]
    public async Task<IActionResult> EditComment(CommentEditResponse response)
    {
        var comment = new CommentViewModel
        {
            Text = response.Content,
        };
        var methods = await permissions.GetMethodsAsync();
        await methods.EditCommentAsync(response.CommentId, comment);

        return Ok(new { Message = $"Комментарий Id: {response.CommentId} обновлен" });
    }

    [HttpPost("delete-comment")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var methods = await permissions.GetMethodsAsync();
        await methods.DeleteCommentAsync(id);

        return Ok(new { Message = $"Комментарий Id: {id} удален" });
    }

    [HttpPost("delete-article")]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        var methods = await permissions.GetMethodsAsync();
        await methods.DeleteArticleAsync(id);

        return Ok(new { Message = $"Статья Id: {id} удалена" });
    }

    [HttpPost("update-your-profile")]
    public async Task<IActionResult> UpdateUserProfile(UserEditRequest request)
    {
        var model = new UpdateUserViewModel()
        {
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            FirstName = request.FirstName,
            LastName = request.LastName,
            ConfirmPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        var methods = await permissions.GetMethodsAsync();
        var result = await methods.EditUserProfileAsync(model, false);
        if (!result.Succeeded)
        {
            Log.Error("AccountManagerController: Ошибка обновления профиля пользователя {UserId}: {Errors}",
                model.UserId,
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return BadRequest(new { Message = "Ошибка обновления профиля", Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = $"Профиль Id: {model.UserId} успешно обновлён" });
    }

    [HttpPost("update-user-role")]
    public async Task<IActionResult> UpdateUserRole(UserRoleUpdateRequest request)
    {
        var isAdminEditingOtherUser = User.IsInRole("Administrator") &&
                                      User.FindFirstValue(ClaimTypes.NameIdentifier) != request.UserId;

        if (!isAdminEditingOtherUser)
            return StatusCode(403, new { Message = "У вас нет прав на эту операцию" });

        var methods = await permissions.GetMethodsAsync();
        var (editingUser, userRoles) = await methods.GetUserInfoAsync(request.UserId);

        var model = new UpdateUserViewModel()
        {
            UserId = request.UserId,
            FirstName = editingUser.FirstName,
            LastName = editingUser.LastName,
            Role = request.UserRole
        };

        var result = await methods.EditUserProfileAsync(model, true);
        if (!result.Succeeded)
        {
            Log.Error("AccountManagerController: Ошибка обновления роли пользователя {UserId}: {Errors}",
                model.UserId,
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return BadRequest(new { Message = "Ошибка обновления роли", Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = $"Роль пользователя Id: {model.UserId} успешно обновлёна на {request.UserRole}" });
    }

    [HttpPost("delete_user_profile")]
    public async Task<IActionResult> DeleteUserProfile(string userId)
    {
        var isAdminDeleteOtherUser = User.IsInRole("Administrator") &&
                                     User.FindFirstValue(ClaimTypes.NameIdentifier) != userId;
        // Проверяем, что пользователь удаляет именно свой профиль
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId && !isAdminDeleteOtherUser)
        {
            return StatusCode(403, new { Message = "Пользователь не существует или удаление запрещено" });
        }

        var methods = await permissions.GetMethodsAsync();

        await methods.DeleteUserAsync(userId);

        return Ok(new { Message = $"Профиль пользователя Id: {userId} удален" });
    }
}