using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Используем SQLite и указываем корректный путь
        optionsBuilder.UseSqlite("Data Source=Db/blog_database.db");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}