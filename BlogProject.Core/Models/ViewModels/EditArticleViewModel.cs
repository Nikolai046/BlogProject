using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.ViewModels;

public class EditArticleViewModel
{
    [Required(ErrorMessage = "Заполните это поле")]
    [Display(Name = "Заголовок")]
    public required string Title { get; set; }

    [Required(ErrorMessage = "Заполните это поле")]
    [Display(Name = "Содержание")]
    public required string Content { get; set; }

    public DateTime? CreatedDate { get; set; }
    public DateTime? UpDateTime { get; set; }
    public int? ArticleId { get; set; }

    public string? Tags { get; set; }
}