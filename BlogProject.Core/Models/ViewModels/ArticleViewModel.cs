using BlogProject.Core.Models.ViewModels.DTO;

namespace BlogProject.Core.Models.ViewModels;

public class ArticleViewModel
{
    public int ArticleId { get; set; }
    public string Title { get; set; }
    public string? Content { get; set; }
    public string? AuthorFullName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string UserId { get; set; }
    public bool Deletable { get; set; }
    public bool Editable { get; set; }

    public IEnumerable<TagViewModel>? Tag { get; set; }
    public IEnumerable<CommentViewModel>? Comments { get; set; }
}