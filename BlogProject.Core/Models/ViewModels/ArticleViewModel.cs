using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.ViewModels;

public class ArticleViewModel
{
    public int ArticleId { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.")]
    [Required(ErrorMessage = "Заполните это поле")]
    [Display(Name = "Заголовок")]
    public string Title { get; set; } = string.Empty;

    [StringLength(10000, MinimumLength = 2, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.")]
    [Required(ErrorMessage = "Заполните это поле")]
    [Display(Name = "Содержагие статьи")]
    public string? Content { get; set; }

    public string? AuthorFullName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UserId { get; set; }
    public bool Deletable { get; set; }
    public bool Editable { get; set; }

    public IEnumerable<TagViewModel>? Tag { get; set; }
    //public List<TagViewModel> Tag { get; set; } = [];
    public IEnumerable<CommentViewModel>? Comments { get; set; }
}