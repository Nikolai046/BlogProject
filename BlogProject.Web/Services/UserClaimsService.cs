using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Security.Claims;
using BlogProject.Core.Models.ViewModels;

namespace BlogProject.Web.Services;

public class UserClaimsService(
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor,
    GetUserPermissions permissions)
{
    // Используем IHttpContextAccessor для доступа к HttpContext
    private HttpContext HttpContext => httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext недоступен.");

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
            new Claim(ClaimTypes.GivenName, $"{userInfo.Item1.FirstName} {userInfo.Item1.LastName}"),
            new Claim("ArticlesCount", userInfo.Item1.ArticleCount.ToString())
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

    public async Task RefreshUserClaims(UserViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId!);
        if (user == null)
        {
            Log.Error("UserClaimsService: При обновлении клаймов пользователь: {UserId} не найден", model.UserId);
            return;
        }

        // Обновляем claim ArticlesCount в базе данных
        var existingClaim = (await userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "ArticlesCount");
        if (existingClaim != null)
        {
            await userManager.RemoveClaimAsync(user, existingClaim);
        }
        await userManager.AddClaimAsync(user, new Claim("ArticlesCount", model.ArticleCount.ToString()));

        // Если текущий пользователь совпадает с целевым, обновляем сессию
        var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (model.UserId == currentUserId)
        {
            // Создаём новый ClaimsPrincipal
            var principal = await CreateUserPrincipalAsync(user);

            // Обновляем claim ArticlesCount в ClaimsPrincipal
            var identity = (ClaimsIdentity)principal.Identity!;
            var sessionClaim = identity.FindFirst("ArticlesCount");
            if (sessionClaim != null)
            {
                identity.RemoveClaim(sessionClaim);
            }
            identity.AddClaim(new Claim("ArticlesCount", model.ArticleCount.ToString()));

            // Обновляем claim роли
            var roleClaim = identity.FindFirst(ClaimTypes.Role);
            if (roleClaim != null)
            {
                identity.RemoveClaim(roleClaim);
            }
            var currentRoles = await userManager.GetRolesAsync(user);
            foreach (var role in currentRoles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // Получаем текущее значение IsPersistent
            var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (authResult == null)
            {
                Log.Warning("UserClaimsService: Результат аутентификации равен null для пользователя {UserId}", model.UserId);
                return;
            }
            var isPersistent = authResult?.Properties?.IsPersistent ?? false;

            // Обновляем сессию с сохранением IsPersistent
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = isPersistent
            });
            Log.Information("UserClaimsService: Обновленная сессия для текущего пользователя: {Email}, Роли: {Roles}", user.Email, string.Join(", ", currentRoles));
        }
        else
        {
            Log.Information("UserClaimsService: Обновление SecurityStamp для пользователя: {Email}", user.Email);
            // Для другого пользователя обновляем SecurityStamp, чтобы завершить его сессии
            await userManager.UpdateSecurityStampAsync(user);
        }
    }

    public async Task<ClaimsPrincipal> SaveNewClaimAsync(User user)
    {
        var principal = await CreateUserPrincipalAsync(user);
        var claimResult = await userManager.AddClaimsAsync(user, claims);
        if (!claimResult.Succeeded)
        {
            var errorMessages = string.Join("; ", claimResult.Errors.Select(e => e.Description));
            Log.Error("UserClaimsService: Ошибка создания клайма для пользователя {FirstName} {LastName}: {Errors}", user.FirstName, user.LastName, errorMessages);
        }
        else
        {
            Log.Information("UserClaimsService: Клайм для пользователя {user.FirstName} {user.LastName} успешно создан", user.FirstName,
             user.LastName);
        }
        return principal;

    }
}