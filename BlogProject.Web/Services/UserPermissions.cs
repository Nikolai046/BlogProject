using System.Security.Claims;
using BlogProject.Data;
using BlogProject.Data.Entities;
using BlogProject.Data.Methods;
using Microsoft.AspNetCore.Identity;

namespace BlogProject.Web.Services;

public class UserPermissions
{
    private readonly ClaimsPrincipal _user;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public UserPermissions(ClaimsPrincipal user, ApplicationDbContext context, UserManager<User> userManager)
    {
        _user = user;
        _context = context;
        _userManager = userManager;
    }

    public IMethods GetMethods()
    {
        var userId = _user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (_user.IsInRole("Administrator"))
            return new AdministratorMethods(_context, userId, _userManager);
        if (_user.IsInRole("Moderator"))
            return new ModeratorMethods(_context, userId, _userManager);
        return new UserMethods(_context, userId, _userManager);
    }
}