using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace BlogProject.Data.Seeder;

public class TestDataGenerator(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
{
    public async Task Generate()
    {
        if (await userManager.Users.AnyAsync())
            return;

        // Создаем роли, если они не существуют
        await EnsureRolesCreated();

        var usersWithRoles = new List<(User User, string Role, string Password)>
        {
            (
                new() {
                    FirstName = "Ivan",
                    LastName = "Ivanov",
                    Email = "ivan.ivanov@example.com",
                    UserName = "ivan.ivanov@example.com"
                },
                "Administrator",
                "123456"
            ),
            (
                new() {
                    FirstName = "Petr",
                    LastName = "Petrov",
                    Email = "petr.petrov@example.com",
                    UserName = "petr.petrov@example.com"
                },
                "Moderator",
                "123456"
            ),
            (
                new() {
                    FirstName = "Sidor",
                    LastName = "Sidorov",
                    Email = "sidor.sidorov@example.com",
                    UserName = "sidor.sidorov@example.com"
                },
                "User",
                "123456"
            )
        };

        foreach (var (user, role, password) in usersWithRoles)
        {
            // Создаем пользователя
            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                LogErrors("Ошибка создания пользователя", result.Errors);
                continue;
            }

            // Назначаем роль
            var roleResult = await userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                LogErrors($"Ошибка назначения роли {role}", roleResult.Errors);
            }
            else
            {
                Log.Information(
                    "Пользователь {FirstName} {LastName} создан с ролью {Role}",
                    user.FirstName, user.LastName, role);
            }

            // Создаем claims для пользователя
            await ClaimsCreated(user, role);
        }
    }

    private async Task EnsureRolesCreated()
    {
        var roles = new[] { "Administrator", "Moderator", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Log.Information("Создана роль: {Role}", role);
            }
        }
    }

    private static void LogErrors(string prefix, IEnumerable<IdentityError> errors)
    {
        foreach (var error in errors)
        {
            Log.Error("{Prefix}: {Code} - {Description}", prefix, error.Code, error.Description);
            Console.WriteLine($"{prefix}: {error.Code} - {error.Description}");
        }
    }

    private async Task ClaimsCreated(User user, string userRole)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, userRole),
            new("ArticlesCount", "0"),
        };

        var claimResult = await userManager.AddClaimsAsync(user, claims);
        if (!claimResult.Succeeded)
        {
            Log.Error($"Ошибка создания клайма для пользователя {user.FirstName} {user.LastName}, {claimResult.Errors}");
        }
        else
        {
            Log.Information(
                "Клайм для пользователя {FirstName} {LastName} успешно создан ", user.FirstName, user.LastName);
        }
    }
}