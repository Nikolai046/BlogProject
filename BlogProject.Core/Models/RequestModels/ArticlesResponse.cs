using BlogProject.Core.Models.ViewModels;

namespace BlogProject.Core.Models.RequestModels;

public class ArticlesResponse
{
    public List<ArticleViewModel>? Articles { get; set; }
    public bool HasMore { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; } = 0;
}