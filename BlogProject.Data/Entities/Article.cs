using System.ComponentModel.DataAnnotations;

namespace BlogProject.Data.Entities;


/// <summary>
/// Представляет статью с заголовком, содержимым, датами создания и обновления, 
/// а также связями с пользователем, комментариями и тегами статьи.
/// </summary>
public class Article
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    // Связи
    public int UserId { get; set; }
    public User User { get; set; }

    public ICollection<Tag> Tags { get; set; } // Связь многие ко многим
    public ICollection<Comment> Comments { get; set; } // Связь один ко многим
}