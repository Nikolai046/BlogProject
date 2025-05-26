using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Data.Seeder;

public class TestDataGenerator
{
    private readonly UserManager<User> _userManager;

    public TestDataGenerator(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task Generate()
    {
        var isDatabaseEmpty = !await _userManager.Users.AnyAsync();
        if (!isDatabaseEmpty)
            return;

        var users = new List<User>
    {
        new User { FirstName = "Ivan", LastName = "Ivanov", Email = "ivan.ivanov@example.com", RoleId = 1},
        new User { FirstName = "Petr", LastName = "Petrov", Email = "petr.petrov@example.com", RoleId = 3},
        new User { FirstName = "Sidor", LastName = "Sidorov", Email = "sidor.sidorov@example.com", RoleId = 3}
    };

        foreach (var user in users)
        {
            var result = await _userManager.CreateAsync(user, "123456");
            if (!result.Succeeded)
            {
                // Логируйте или выводите ошибки
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"{error.Code}: {error.Description}");
                }
            }
        }
    }
}