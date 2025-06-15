using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Data.Entities;

public class User : IdentityUser
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public override string? Email { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    public virtual ICollection<Article>? Articles { get; set; }

    public virtual ICollection<Comment>? Comments { get; set; }

    public string GetFullName()
    {
        return $"{FirstName} {LastName}";
    }
}