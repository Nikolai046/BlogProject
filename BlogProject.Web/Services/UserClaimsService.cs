using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using BlogProject.Core.CustomException;
using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Serilog;
using InvalidOperationException = System.InvalidOperationException;

namespace BlogProject.Web.Services;

public class UserClaimsService(
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor,
    GetUserPermissions permissions)
{
    // Используем IHttpContextAccessor для доступа к HttpContext
    private HttpContext HttpContext => httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available.");
    private List<Claim> claims = [new Claim("ArticlesCount", "0")];

    public async Task<ClaimsPrincipal> CreateUserPrincipalAsync(User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var userInfo = await permissions.GetMethods(user, roles).GetUserInfoAsync();

        claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.UserName !),
            new Claim(ClaimTypes.GivenName, $"{userInfo.FirstName} {userInfo.LastName}"),
            new Claim("ArticlesCount", userInfo.ArticleCount.ToString())
        ];

        // Добавляем все роли пользователя
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var principal = new ClaimsPrincipal(identity);

        return principal;
    }

    public async Task UpdateArticlesCountClaim(string userId, int newCount)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return;

        // Создаем полностью новый principal с обновленными claims
        var principal = await CreateUserPrincipalAsync(user);

        // Обновляем claim ArticlesCount в новом principal
        var identity = (ClaimsIdentity)principal.Identity!;
        var existingClaim = identity.FindFirst("ArticlesCount");

        if (existingClaim != null)
        {
            identity.RemoveClaim(existingClaim);
        }
        identity.AddClaim(new Claim("ArticlesCount", newCount.ToString()));


        // Обновляем аутентификацию через HttpContext
        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = false
        });
    }
    public async Task RefreshUserClaims(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var principal = await CreateUserPrincipalAsync(user);
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = false
            });
        }
        else
        {
            throw new NotFoundException($"Пользователь с Id={userId} для обновления клайма");
        }
    }

    public async Task<ClaimsPrincipal> SaveNewClaimAsync(User user)
    {
        var principal = await CreateUserPrincipalAsync(user);
        var claimResult = await userManager.AddClaimsAsync(user, claims);
        if (!claimResult.Succeeded)
        {
            Log.Error($"Ошибка создания клайма для пользователя {user.FirstName} {user.LastName}, {claimResult.Errors}");
        }
        else
        {
            Log.Information($"Клайм для пользователя {user.FirstName} {user.LastName} успешно создан");
        }

        return principal;
    }

}