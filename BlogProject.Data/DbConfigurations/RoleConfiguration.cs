/*
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogProject.Data.DbConfigurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Указываем имя таблицы
        builder.ToTable("Roles");

        // Настраиваем первичный ключ
        builder.HasKey(r => r.Id);

        // Настраиваем свойства
        builder.Property(r => r.Name)
            .IsRequired() // Поле обязательно
            .HasMaxLength(50); // Максимальная длина строки 50 символов

        // Добавляем начальные данные (опционально)
        builder.HasData(
            new Role { Id = 1, Name = "Administrator" },
            new Role { Id = 2, Name = "Moderator" },
            new Role { Id = 3, Name = "User" }
        );
    }
}
*/