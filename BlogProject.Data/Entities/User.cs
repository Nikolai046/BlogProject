using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Data.Entities;

public class User : IdentityUser
{
    [Required]
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string? LastName { get; set; }
    [Required]
    [EmailAddress]
    public override required string? Email
    {
        get => base.Email!;
        set
        {
            base.Email = value;
            // Устанавливаем UserName только если он еще не задан
            if (string.IsNullOrEmpty(base.UserName))
            {
                base.UserName = value;
            }
        }
    }

    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    public virtual ICollection<Article>? Articles { get; set; }

    public virtual ICollection<Comment>? Comments { get; set; }

    public string GetFullName()
    {
        return $"{FirstName} {LastName}";
    }
}
