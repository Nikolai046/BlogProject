using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BlogProject.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
{
    public new DbSet<User> Users { get; set; }

    // public new DbSet<Role> Roles { get; set; }
    public DbSet<Article> Articles { get; set; }

    public DbSet<Tag> Tags { get; set; }
    public DbSet<Comment> Comments { get; set; }

    /// <summary>
    /// Настраивает модели Entity Framework, применяя конфигурации из сборки, в которой определен контекст базы данных.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        try
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                "ApplicationDbContext: Ошибка при конфигурировании моделей в OnModelCreating. " +
                "Тип исключения: {ExceptionType}, Сообщение: {Message}, StackTrace: {StackTrace}",
                ex.GetType().FullName, ex.Message, ex.StackTrace); throw;
        }
    }

    public override int SaveChanges()
    {
        try
        {
            return base.SaveChanges();
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                "ApplicationDbContext: Ошибка при сохранении изменений в базе данных. " +
                "Тип исключения: {ExceptionType}, Сообщение: {Message}, StackTrace: {StackTrace}",
                ex.GetType().FullName, ex.Message, ex.StackTrace); throw;
        }
    }
}