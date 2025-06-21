using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.RequestModels;

public class CommentEditResponse
{
    [Required(ErrorMessage = "Поле не может быть пустым.")]
    public int CommentId { get; set; }

    [StringLength(10000, MinimumLength = 2, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.")]
    [Required(ErrorMessage = "Поле не может быть пустым.")]
    public string? Content { get; set; } = string.Empty;
}