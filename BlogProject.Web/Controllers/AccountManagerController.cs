using BlogProject.Core.Models.ViewModels;
using BlogProject.Data.Entities;
using BlogProject.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using System.Text.Json;

namespace BlogProject.Web.Controllers;

[Authorize]
[Route("[controller]")]
public class AccountManagerController(GetUserPermissions permissions, UserClaimsService claimsService) : Controller
{
    [HttpGet("main_page")]
    public async Task<IActionResult> MainPage(int page = 1, int pageSize = 10, int? articleId = null)
    {
        var methods = permissions.GetMethods();
        var result = await methods.GetAllArticles(page, pageSize);
        ViewBag.ArticleId = articleId;

        return View(result);
    }

    [HttpGet("articles_by_author")]
    public async Task<IActionResult> ArticlesByAuthor(string userId, int page = 1, int pageSize = 10, int? articleId = null)
    {
        var methods = permissions.GetMethods();
        var result = await methods.GetArticlesByUserId(userId, page, pageSize);
        ViewBag.ArticleId = articleId;
        ViewBag.UserId = userId;
        return View(result);
    }

    [HttpGet("articles_by_username")]
    public async Task<IActionResult> ArticlesByUsername(string userName, int page = 1, int pageSize = 10, int? articleId = null)
    {
        if (string.IsNullOrEmpty(userName)) return View(null);
        var methods = permissions.GetMethods();
        var userId = await methods.FindUserIdsByNameAsync(userName);
        var result = await methods.GetArticlesByUserId(userId, page, pageSize);
        ViewBag.ArticleId = articleId;
        ViewBag.UserId = userId;
        return View(result);
    }

    [HttpGet("articles_by_tags")]
    public async Task<IActionResult> ArticlesByTags(string tagList, int page = 1, int pageSize = 10, int? articleId = null)
    {
        if (string.IsNullOrEmpty(tagList)) return View(null);

        var tags = JsonSerializer.Deserialize<List<string>>(tagList) ?? [];

        var methods = permissions.GetMethods();
        var result = await methods.GetAllArticlesByTag(tags, page, pageSize);
        ViewBag.ArticleId = articleId; // Устанавливаем ViewBag.ArticleId
        return View(result);
    }


    [HttpGet("create_article")]
    public IActionResult CreateArticle()
    {
        var model = new ArticleViewModel
        {
            AuthorFullName = User.FindFirstValue(ClaimTypes.GivenName),
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Title = string.Empty,
            Content = string.Empty,
            CreatedDate = DateTime.Now,
            Tag = new List<TagViewModel> { new() },
            Comments = new List<CommentViewModel>()
        };
        return View(model);
    }

    [HttpPost("create_article")]
    public async Task<IActionResult> CreateArticle(ArticleViewModel model, string tagList)
    {
        model.CreatedDate = DateTime.Now;
        if (!string.IsNullOrEmpty(tagList))
        {
            var tags = JsonSerializer.Deserialize<List<string>>(tagList);
            model.Tag = tags?
                .Where(t => !string.IsNullOrEmpty(t) && t != "null")
                .Select(t => new TagViewModel { Text = t })
                .ToList() ?? [];
        }
        if (!ModelState.IsValid) return View(model);
        var methods = permissions.GetMethods();
        await methods.CreateArticle(model);

        var user = await methods.GetUserInfoAsync();
        await claimsService.RefreshUserClaims(user);

        return RedirectToAction("MainPage", "AccountManager");
    }

    [HttpGet("edit_article/{id}/{page?}")]
    public IActionResult EditArticle([FromRoute] int id, [FromRoute] int page)
    {
        var methods = permissions.GetMethods();
        var model = methods.GetArticleById(id).Result;
        ViewBag.Page = page;
        return View(model);
    }

    [HttpPost("edit_article/{articleId}/{page?}")]
    public async Task<IActionResult> EditArticle(ArticleViewModel model, string tagList, [FromRoute] int articleId, [FromRoute] int page)
    {
        if (!ModelState.IsValid) return View(model);
        if (!string.IsNullOrEmpty(tagList))
        {
            var tags = JsonSerializer.Deserialize<List<string>>(tagList);
            model.Tag = tags?
                .Where(t => !string.IsNullOrEmpty(t) && t != "null")
                .Select(t => new TagViewModel { Text = t })
                .ToList() ?? [];
        }
        var methods = permissions.GetMethods();
        await methods.EditArticle(articleId, model);

        return RedirectToAction("MainPage", new { page, articleId = model.ArticleId });
    }

    [HttpPost("create_comment")]
    public async Task<IActionResult> CreateComment(int articleId, string text)
    {
        // Добавляем комментарий к статье
        var comment = new CommentViewModel
        {
            Text = text,
        };
        var methods = permissions.GetMethods();
        await methods.CreateComment(articleId, comment);

        var updatedArticle = await methods.GetArticleById(articleId);

        return PartialView("_CommentsPartial", updatedArticle);
    }

    [HttpPost("edit_comment")]
    public async Task<IActionResult> EditComment(int commentId, string text)
    {
        // Добавляем комментарий к статье
        var comment = new CommentViewModel
        {
            Text = text,
        };
        var methods = permissions.GetMethods();
        await methods.EditComment(commentId, comment);

        return Ok();
    }


    [HttpPost("delete_comment")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var methods = permissions.GetMethods();
        await methods.DeleteComment(id);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteArticle(int id, int page)
    {
        var methods = permissions.GetMethods();
        await methods.DeleteArticle(id);

        return RedirectToAction("MainPage", new { page });
    }

    [HttpGet("get_all_users")]
    public async Task<IActionResult> GetAllUsers(int page = 1, int pageSize = 10)
    {
        var methods = permissions.GetMethods();
        var result= await methods.GetAllUsersAsync(page, pageSize);

        return View(result);
    }
}