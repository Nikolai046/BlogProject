using BlogProject.Core.Models.ViewModels;
using BlogProject.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using BlogProject.Data.Entities;

namespace BlogProject.Web.Controllers;

[Authorize]
[Route("[controller]")]
public class AccountManagerController(GetUserPermissions permissions) : Controller
{

    [HttpGet("main_page")]
    public async Task<IActionResult> MainPage(int page = 1, int pageSize = 10, int? articleId = null)
    {
        var methods = permissions.GetMethods();
        var result = await methods.GetAllArticles(page, pageSize);
        ViewBag.ArticleId = articleId;

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

    public async Task<IActionResult> EditComment()
    {
        throw new NotImplementedException();
    }

    [HttpPost("delete_comment")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var methods = permissions.GetMethods();
        await methods.DeleteComment(id);
        return Ok();
    }


    public async Task<IActionResult> DeleteArticle()
    {
        throw new NotImplementedException();
    }
}

