using BlogProject.Core.CustomException;
using BlogProject.Data;
using BlogProject.Data.Entities;
using BlogProject.Data.Methods;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlogProject.Api.Services;

public class GetUserPermissions(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context, UserManager<User> userManager)
{
    public async Task<IMethods> GetMethodsAsync()
    {
        var user = httpContextAccessor.HttpContext?.User
                   ?? throw new NotFoundException("Идентификатор пользователя не может быть пустым.");

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new NotFoundException("Идентификатор пользователя в клайме не найден");

        // Получаем роли пользователя асинхронно через UserManager
        var dbUser = await userManager.FindByIdAsync(userId);
        if (dbUser == null)
            throw new NotFoundException("Пользователь не найден.");

        var roles = await userManager.GetRolesAsync(dbUser);

        if (roles.Contains("Administrator"))
            return new AdministratorMethods(context, userId, userManager);
        if (roles.Contains("Moderator"))
            return new ModeratorMethods(context, userId, userManager);
        return new UserMethods(context, userId, userManager);
    }
}