using BlogProject.Core.CustomException;
using BlogProject.Data;
using BlogProject.Data.Entities;
using BlogProject.Data.Methods;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlogProject.Web.Services;

public class GetUserPermissions(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context, UserManager<User> userManager)
{
    public IMethods GetMethods()
    {
        var user = httpContextAccessor.HttpContext?.User
                   ?? throw new NotFoundException("Идентификатор пользователя не может быть пустым.");

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (user.IsInRole("Administrator"))
            return new AdministratorMethods(context, userId, userManager);
        if (user.IsInRole("Moderator"))
            return new ModeratorMethods(context, userId, userManager);
        return new UserMethods(context, userId, userManager);
    }

    //public async Task<IMethods> GetMethods(User user)
    //{
    //    if (user == null)
    //    {
    //        throw new ArgumentNullException(nameof(user), "User cannot be null");
    //    }

    //    // Получаем все роли пользователя
    //    var userRoles = await userManager.GetRolesAsync(user);

    //    // Определяем приоритет ролей
    //    if (userRoles.Contains("Administrator"))
    //    {
    //        return new AdministratorMethods(context, user.Id, userManager);
    //    }
    //    if (userRoles.Contains("Moderator"))
    //    {
    //        return new ModeratorMethods(context, user.Id, userManager);
    //    }

    //    return new UserMethods(context, user.Id, userManager);

    //}

    // Для вызовов с объектом User (используется в UserClaimsService)
    public IMethods GetMethods(User user, IList<string> roles)
    {
        if (user == null)
            throw new NotFoundException("Идентификатор пользователя не может быть пустым.");

        if (roles.Contains("Administrator"))
            return new AdministratorMethods(context, user.Id, userManager);
        if (roles.Contains("Moderator"))
            return new ModeratorMethods(context, user.Id, userManager);
        return new UserMethods(context, user.Id, userManager);
    }
}