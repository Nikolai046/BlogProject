using System.ComponentModel.DataAnnotations;

namespace BlogProject.Data.Entities;

public class Role
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } // "Admin", "Moderator", "User"

    // Навигационное свойство
    public ICollection<User> Users { get; set; }
}