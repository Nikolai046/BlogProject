using BlogProject.Api.Services;
using BlogProject.Core.Models.RequestModels;
using BlogProject.Core.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        var methods = permissions.GetMethods();
        try
        {
            var (user, roleList) = await methods.GetUserInfoAsync(userId);
            var model = new UserInfoResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                ArticleCount = user.ArticleCount,
                Roles = user.Roles
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
        var methods = permissions.GetMethods();
        try
        {
            var (users, hasMore, lastPage) = await methods.GetAllUsersAsync(page,pageSize);

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
        var methods = permissions.GetMethods();

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
        var methods = permissions.GetMethods();

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
        var methods = permissions.GetMethods();

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

        var methods = permissions.GetMethods();

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

        var methods = permissions.GetMethods();
        await methods.CreateArticleAsync(model);

       return Ok(new { Message = "Статья успешно создана" });
    }

    [HttpPost("edit_article")]
    public async Task<IActionResult> EditArticle(ArticleEditResponse response)
    {
        var tags = response.Tags != null ? Regex.Split(response.Tags, @"\W+").Where(word => !string.IsNullOrEmpty(word)).ToList() : new List<string>();

        var methods = permissions.GetMethods();
        

        var model = new ArticleViewModel()
        {
            Title = response.Title,
            Content = response.Content,
            Tag = tags.Select(tag => new TagViewModel { Text = tag }).ToList(),
        };

        await methods.EditArticleAsync(response.ArticleId, model);
        
        return Ok(new { Message = "Статья успешно отредактирована" });
    }

    [HttpPost("create_comment")]
    public async Task<IActionResult> CreateComment(CommentCreateResponse response)
    {
        var comment = new CommentViewModel
        {
            Text = response.Content
        };
        var methods = permissions.GetMethods();

        await methods.CreateCommentAsync(response.ArticleId, comment);

        return Ok(new { Message = $"Комментарий к статье {response.ArticleId} успешно добавлен" });
    }

}