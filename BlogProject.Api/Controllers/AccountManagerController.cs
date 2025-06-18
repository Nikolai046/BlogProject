using BlogProject.Api.Services;
using BlogProject.Core.Models.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BlogProject.Api.Controllers;


[Route("api/[controller]")]
[Authorize]
[ApiController]
public class AccountManagerController(GetUserPermissions permissions, JwtService jwtService) : ControllerBase
{
    [Authorize]
    [HttpGet("get_all-articles")]
    [ProducesResponseType(typeof(ArticlesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllArticles(int page = 1, int pageSize = 10)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? throw new InvalidOperationException("Идентификатор пользователя не найден в токене.");

        Log.Information("Пользователь {UserId} запросил статьи, страница: {Page}, размер страницы: {PageSize}", userId, page, pageSize);

        var methods = permissions.GetMethods();
        var (articles, hasMore) = await methods.GetAllArticlesAsync(page, pageSize);

        var response = new ArticlesResponse
        {
            Articles = articles,
            HasMore = hasMore,
            Page = page,
            PageSize = pageSize,
        };

        return Ok(response);
    }

}