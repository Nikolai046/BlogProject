namespace BlogProject.Core.Models.ViewModels;

public class CommentViewModel
{
    public int CommentId { get; set; }
    public string? Text { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool Editable { get; set; }
    public bool Deletable { get; set; }
    public string Author { get; set; }
}