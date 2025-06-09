using BlogProject.Core.Models.ViewModels;
using BlogProject.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Serilog;

namespace BlogProject.Web.Controllers;


[Route("[controller]")]
public class AccountManagerController(GetUserPermissions permissions) : Controller
{

    [HttpGet("main_page")]
    public async Task<IActionResult> MainPage(int page = 1, int pageSize = 10)
    {
        var methods = permissions.GetMethods();
        var result = await methods.GetAllArticles(page, pageSize);
        return View(result);

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
                .ToList() ?? new List<TagViewModel>();
        }
        if (!ModelState.IsValid) return View(model);
        var methods = permissions.GetMethods();
        await methods.CreateArticle(model);

        return RedirectToAction("MainPage", "AccountManager");

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
            CreatedDate = DateTime.UtcNow,
            Tag = new List<TagViewModel> { new TagViewModel() },
            Comments = new List<CommentViewModel>()
        };
        return View(model);
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
    public async Task<IActionResult> EditComment(int articleId, int commentId, string text)
    {

        // Добавляем комментарий к статье
        var comment = new CommentViewModel
        {
            Text = text,
        };
        var methods = permissions.GetMethods();
        await methods.EditComment(commentId, comment);

        //var updatedArticle = await methods.GetArticleById(articleId);

        //return PartialView("_CommentsPartial", updatedArticle);
        return Ok();

    }

    public async Task<IActionResult> EditComment()
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> DeleteComment()
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> EditArticle()
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> DeleteArticle()
    {
        throw new NotImplementedException();
    }
}

