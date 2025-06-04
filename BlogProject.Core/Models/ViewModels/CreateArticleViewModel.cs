namespace BlogProject.Core.Models.ViewModels;

public class CreateArticleViewModel
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? UserId { get; set; }
    public bool Deletable { get; set; }
    public bool Editable { get; set; }

    public IEnumerable<TagViewModel>? Tag { get; set; }
    public IEnumerable<CommentViewModel>? Comments { get; set; }
}