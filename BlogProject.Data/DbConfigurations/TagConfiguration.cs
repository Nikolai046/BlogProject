using BlogProject.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogProject.Data.DbConfigurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        // Указываем имя таблицы
        builder.ToTable("Tags");

        // Настраиваем первичный ключ
        builder.HasKey(t => t.Id);

        // Настраиваем свойства
        builder.Property(t => t.Name)
            .IsRequired() // Поле обязательно
            .HasMaxLength(100); // Максимальная длина строки 100 символов

    }
}