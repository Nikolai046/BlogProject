using BlogProject.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogProject.Data.DbConfigurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        // Указываем имя таблицы
        builder.ToTable("Articles");

        // Настраиваем первичный ключ
        builder.HasKey(a => a.Id);

        // Настраиваем свойства
        builder.Property(a => a.Title)
            .IsRequired()         // Поле обязательно
            .HasMaxLength(200);   // Максимальная длина строки 200 символов

        builder.Property(a => a.Content)
            .IsRequired();        // Поле обязательно

        builder.Property(a => a.CreatedDate)
            .IsRequired();        // Поле обязательно

        // Связь с автором (User)
        builder.HasOne(a => a.User)                // Одна статья имеет одного автора
            .WithMany(u => u.Articles)           // У пользователя может быть много статей
            .HasForeignKey(a => a.UserId)      // Внешний ключ
            .OnDelete(DeleteBehavior.Restrict);  // Запрещаем каскадное удаление

        // Связь один ко многим с Comment
        builder.HasMany(a => a.Comments)            // У статьи может быть много комментариев
            .WithOne(c => c.Article)             // Один комментарий привязан к одной статье
            .HasForeignKey(c => c.ArticleId)     // Внешний ключ
            .OnDelete(DeleteBehavior.Cascade);   // Каскадное удаление комментариев при удалении статьи
    }
}