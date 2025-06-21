namespace BlogProject.Core.Models.RequestModels;

public class UserInfoResponse
{
    public string? UserId { get; set; }
    public string? FullName { get; set; }

    public List<string> Roles { get; set; } = [];
    public string? Email { get; set; }
    public int ArticleCount { get; set; } = 0;
}