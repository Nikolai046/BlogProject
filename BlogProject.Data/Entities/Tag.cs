using System.ComponentModel.DataAnnotations;

namespace BlogProject.Data.Entities;

public class Tag
{
    public int Id { get; set; }

    public required string Name { get; set; }

    // Связь многие-ко-многим
    public virtual ICollection<Article>? Articles { get; set; }
}