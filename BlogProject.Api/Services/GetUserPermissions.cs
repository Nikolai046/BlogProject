using BlogProject.Core.CustomException;
using BlogProject.Data;
using BlogProject.Data.Entities;
using BlogProject.Data.Methods;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlogProject.Api.Services;

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
}