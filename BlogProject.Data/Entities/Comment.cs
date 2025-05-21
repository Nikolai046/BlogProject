using System.ComponentModel.DataAnnotations;

namespace BlogProject.Data.Entities;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public string Text { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    // Связи
    public int UserId { get; set; }
    public User User { get; set; }

    public int ArticleId { get; set; }
    public Article Article { get; set; }

}