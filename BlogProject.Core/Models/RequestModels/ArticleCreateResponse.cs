using BlogProject.Core.Models.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.RequestModels;

public class ArticleCreateResponse
{
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.")]
        [Required(ErrorMessage = "Поле не может быть пустым.")]
        [Display(Name = "Заголовок")]
        public string Title { get; set; } = string.Empty;

        [StringLength(10000, MinimumLength = 2, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.")]
        [Required(ErrorMessage = "Поле не может быть пустым.")]
        [Display(Name = "Содержание статьи")]
        public string? Content { get; set; } = string.Empty;

         [Display(Name = "Теги статьи, разделенные пробелом")]
        public string? Tags { get; set; } = string.Empty;

}