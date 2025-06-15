using BlogProject.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogProject.Data.DbConfigurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        // Указываем имя таблицы
        builder.ToTable("Comments");

        // Настраиваем первичный ключ
        builder.HasKey(c => c.Id);

        // Настраиваем свойства
        builder.Property(c => c.Text)
            .IsRequired();        // Поле обязательно

        builder.Property(c => c.CreatedDate)
            .IsRequired();        // Поле обязательно

        // Связь с автором (User)
        builder.HasOne(c => c.User)                // Один комментарий имеет одного автора
            .WithMany(u => u.Comments)           // У пользователя может быть много комментариев
            .HasForeignKey(c => c.UserId)      // Внешний ключ
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);  // Запрещаем каскадное удаление

        // Связь с Article
        builder.HasOne(c => c.Article)              // Один комментарий привязан к одной статье
            .WithMany(a => a.Comments)           // У статьи может быть много комментариев
            .HasForeignKey(c => c.ArticleId)     // Внешний ключ
            .OnDelete(DeleteBehavior.Cascade);   // Каскадное удаление при удалении статьи
    }
}