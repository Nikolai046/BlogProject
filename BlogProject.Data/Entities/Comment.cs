using System.ComponentModel.DataAnnotations;

namespace BlogProject.Data.Entities;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public required string Text { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    [Required]
    public string? UserId { get; set; } = string.Empty;
    public int ArticleId { get; set; }

    // Связи
    public virtual User? User { get; set; }
    public virtual Article? Article { get; set; }
}
