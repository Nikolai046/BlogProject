namespace BlogProject.Data.Entities;

public class Comment
{
    public int Id { get; set; }

    public string Text { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }

    public string? UserId { get; set; } = string.Empty;

    public int ArticleId { get; set; }

    // Связи
    public virtual User? User { get; set; }

    public virtual Article? Article { get; set; }
}