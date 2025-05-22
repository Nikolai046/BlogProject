using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Data.Entities;

public class User : IdentityUser
{
    public string Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    // Связь с Role
    public int RoleId { get; set; }
    public Role Role { get; set; }

    // Навигационные свойства
    public ICollection<Article> Articles { get; set; }
    public ICollection<Comment> Comments { get; set; }
}