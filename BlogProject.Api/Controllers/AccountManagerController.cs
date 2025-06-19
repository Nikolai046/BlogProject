using BlogProject.Api.Services;
using BlogProject.Core.Models.RequestModels;
using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;
using BlogProject.Core.CustomException;
using InvalidOperationException = System.InvalidOperationException;

namespace BlogProject.Api.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class AccountManagerController(GetUserPermissions permissions) : ControllerBase
{
  
    [HttpGet("get_all-articles")]
    public async Task<IActionResult> GetAllArticles(int page = 1, int pageSize = 10)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? throw new InvalidOperationException("Идентификатор пользователя не найден в токене.");

        Log.Information("Пользователь {UserId} запросил статьи, страница: {Page}, размер страницы: {PageSize}", currentUserId, page, pageSize);

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
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                            ?? throw new InvalidOperationException("Идентификатор пользователя не найден в токене.");

        Log.Information("Пользователь {UserId} запросил статьи, страница: {Page}, размер страницы: {PageSize}", currentUserId, page, pageSize);

        var methods = permissions.GetMethods();
        var (articles, hasMore, lastPage) = await methods.GetArticlesByUserIdAsync(userId, page, pageSize);
        if (articles == null || !articles.Any())
        {
            Log.Warning("Пользователь {UserId} запросил статьи для пользователя: {RequestedUserId}, но статьи не найдены.", currentUserId, userId);
            return NotFound(new { ErrorMessage = "Статьи не найдены для указанного пользователя." });
        }

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
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                            ?? throw new InvalidOperationException("Идентификатор пользователя не найден в токене.");

        if (string.IsNullOrEmpty(userName)) throw new NotFoundException("Поле userName не может быть пустым");

        var methods = permissions.GetMethods();

        var userId = await methods.FindUserIdsByNameAsync(userName);

        var (articles, hasMore, lastPage) = await methods.GetArticlesByUserIdAsync(userId, page, pageSize);

        if (articles == null || !articles.Any())
        {
            Log.Warning("Пользователь {UserId} запросил статьи для пользователя {RequestedUserId}, но статьи не найдены.", currentUserId, userId);
            throw new NotFoundException ("Статьи пользователя");
        }

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

    //[HttpGet("articles_by_tags")]
    //public async Task<IActionResult> ArticlesByTags(string tagList, int page = 1, int pageSize = 10, int? articleId = null)
    //{
    //    if (string.IsNullOrEmpty(tagList)) return View(null);

    //    var tags = JsonSerializer.Deserialize<List<string>>(tagList) ?? [];

    //    var methods = permissions.GetMethods();
    //    var result = await methods.GetAllArticlesByTagAsync(tags, page, pageSize);
    //    ViewBag.ArticleId = articleId; // Устанавливаем ViewBag.ArticleId
    //    return View(result);
    //}




}