namespace BlogProject.Core.Models.ViewModels;

public class UserViewModel
{
    public required string UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int ArticleCount { get; set; } = 0;
    public bool Deletable { get; set; }
}