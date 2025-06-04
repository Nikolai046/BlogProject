using BlogProject.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BlogProject.Core.Models.ViewModels;

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
    public async Task<IActionResult> CreateComment(ArticleViewModel model)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    public async Task<IActionResult> CreateComment()
    {
        throw new NotImplementedException();
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

