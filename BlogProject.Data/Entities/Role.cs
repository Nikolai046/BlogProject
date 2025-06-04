//using Microsoft.AspNetCore.Identity;
//using System.ComponentModel.DataAnnotations;

//namespace BlogProject.Data.Entities;

//public class Role : IdentityRole<string>
//{
//    public int Id { get; set; }

//    [Required]
//    [MaxLength(50)]
//    public required string Name { get; set; } // "Admin", "Moderator", "User"

//    // Навигационное свойство
//    public virtual ICollection<User>? Users { get; set; }
//}
