namespace BlogProject.Core.Models.ViewModels;

public class UserViewModel
{
    public string? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int ArticleCount { get; set; } = 0;
    public bool Deletable { get; set; }
    public List<string> Roles { get; set; } = [];
    public IEnumerable<TagViewModel>? Tag { get; set; }
}