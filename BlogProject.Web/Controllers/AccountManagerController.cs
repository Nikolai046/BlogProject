using BlogProject.Core.Models.ViewModels;
using BlogProject.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    [HttpPost]
    public async Task<IActionResult> CreateArticle(ArticleViewModel model)
    {
        throw new NotImplementedException();
    }

    [HttpGet("create_article")]
    public async Task<IActionResult> CreateArticle()
    {
        var model = new ArticleViewModel
        {
            AuthorFullName = User.FindFirstValue("FullName") ?? User.Identity.Name,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Title = string.Empty,
            Content = string.Empty,
            CreatedDate = DateTime.UtcNow,
            Tag = new List<TagViewModel> { new TagViewModel() }, // Начальный тег
            Comments = new List<CommentViewModel>() // Пустой список комментариев
        };
        return View(model);
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

