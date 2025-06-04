using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BlogProject.Data;
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Создаем конфигурацию вручную
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath((Path.Combine(Directory.GetCurrentDirectory(), "..", "BlogProject.Web")))
            .AddJsonFile("appsettings.json")
            .Build();

        // Получаем строку подключения
        string connectionString = configuration.GetConnectionString("DefaultConnection")
                                  ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Настраиваем контекст
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

