using BlogProject.Core.Models.ViewModels;
using BlogProject.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        var result = await methods.GetAllArticlesAsync(page, pageSize);
        ViewBag.ArticleId = articleId;

        return View(result);
    }

    [HttpGet("articles_by_author")]
    public async Task<IActionResult> ArticlesByAuthor(string userId, int page = 1, int pageSize = 10, int? articleId = null)
    {
        var methods = permissions.GetMethods();
        var result = await methods.GetArticlesByUserIdAsync(userId, page, pageSize);
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
        var result = await methods.GetArticlesByUserIdAsync(userId, page, pageSize);
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
        var result = await methods.GetAllArticlesByTagAsync(tags, page, pageSize);
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
            Tag = [new()],
            Comments = []
        };
        return View(model);
    }


    [HttpPost("create_article")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateArticle(ArticleViewModel model, string tagList = null!)
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
        await methods.CreateArticleAsync(model);

        // Обновляем количество статей в claims
        var user = await methods.GetUserInfoAsync();
        await claimsService.RefreshUserClaims(user.Item1);

        return RedirectToAction("MainPage", "AccountManager");
    }

    [HttpGet("edit_article/{id}/{page?}")]
    public IActionResult EditArticle([FromRoute] int id, [FromRoute] int page)
    {
        var methods = permissions.GetMethods();
        var model = methods.GetArticleByIdAsync(id).Result;
        ViewBag.Page = page;
        return View(model);
    }

    [HttpPost("edit_article/{articleId}/{page?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditArticle(ArticleViewModel model, [FromRoute] int articleId,
        [FromRoute] int page, string tagList = null!)
    {
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
        await methods.EditArticleAsync(articleId, model);

        return RedirectToAction("MainPage", new { page, articleId = model.ArticleId });
    }

    [HttpPost("create_comment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateComment(int articleId, string text)
    {
        var comment = new CommentViewModel
        {
            Text = text,
        };
        var methods = permissions.GetMethods();
        await methods.CreateCommentAsync(articleId, comment);

        var updatedArticle = await methods.GetArticleByIdAsync(articleId);

        return PartialView("_CommentsPartial", updatedArticle);
    }


    [HttpPost("edit_comment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditComment(int commentId, string text)
    {
        var comment = new CommentViewModel
        {
            Text = text,
        };
        var methods = permissions.GetMethods();
        await methods.EditCommentAsync(commentId, comment);

        return Ok();
    }


    [HttpPost("delete_comment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var methods = permissions.GetMethods();
        await methods.DeleteCommentAsync(id);
       return Ok();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteArticle(int id, int page)
    {
        var methods = permissions.GetMethods();
        var user = await methods.GetUserInfoByArticleIdAsync(id);
        await methods.DeleteArticleAsync(id);

        // Обновляем количество статей в claims
        user.ArticleCount--;
        await claimsService.RefreshUserClaims(user);

        return RedirectToAction("MainPage", new { page });
    }

    [HttpGet("get_all_users")]
    public async Task<IActionResult> GetAllUsers(int page = 1, int pageSize = 10)
    {
        var methods = permissions.GetMethods();
        var result = await methods.GetAllUsersAsync(page, pageSize);

        return View(result);
    }

    [HttpGet("update_user_profile")]
    public async Task<IActionResult> UpdateUserProfile(string userid)
    {
        var methods = permissions.GetMethods();

        var query = await methods.GetUserInfoAsync(userid);
        if (query.Item1 == null) return NotFound();

        var model = new UpdateUserViewModel()
        {
            UserId = query.Item1.UserId,
            FirstName = query.Item1.FirstName,
            LastName = query.Item1.LastName,
            Role = query.Item1.Roles.FirstOrDefault(),
            Availableroles = query.Item2!
        };

        return View(model);

    }

    [HttpPost("update_user_profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserProfile(UpdateUserViewModel model)
    {
        var isAdminEditingOtherUser = User.IsInRole("Administrator") &&
                                      User.FindFirstValue(ClaimTypes.NameIdentifier) != model.UserId;

        if (!ModelState.IsValid && !isAdminEditingOtherUser)
        {
            return NotFound();
        }

        var methods = permissions.GetMethods();

        var result = await methods.EditUserProfileAsync(model, isAdminEditingOtherUser);
        if (!result.Succeeded)
        {
            Log.Error("AccountManagerController: Ошибка обновления профиля пользователя {UserId}: {Errors}",
                model.UserId,
                string.Join("; ", result.Errors.Select(e => e.Description)));
            TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction("UpdateUserProfile", new { userId = model.UserId });
        }
        var user = await methods.GetUserInfoAsync(model.UserId);

        // Обновляем сессию
        await claimsService.RefreshUserClaims(user.Item1);

        TempData["Success"] = "Профиль успешно обновлён";
        return RedirectToAction("UpdateUserProfile", new { userId = model.UserId });
    }

    [HttpPost("delete_user_profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserProfile(string userId, int page=1)
    {
        var isAdminDeleteOtherUser = User.IsInRole("Administrator") &&
                                     User.FindFirstValue(ClaimTypes.NameIdentifier) != userId;
        // Проверяем, что пользователь удаляет именно свой профиль
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId && !isAdminDeleteOtherUser)
        {
            return Forbid();
        }

        var methods = permissions.GetMethods();
        try
        {
            await methods.DeleteUserAsync(userId);

            // Если это текущий пользователь, то нужно выйти из системы
            if (!isAdminDeleteOtherUser)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("GetAllUsers", new{page});
        }
        catch (Exception ex)
        {
            Log.Error(ex, "AccountManagerController: Ошибка удаления профиля {UserId}", userId);
            TempData["Error"] = "Не удалось удалить профиль: " + ex.Message;
            return RedirectToAction("UpdateUserProfile", new { userid = userId });
        }
    }
}


