﻿namespace BlogProject.Data.Entities;

/// <summary>
/// Представляет статью с заголовком, содержимым, датами создания и обновления,
/// а также связями с пользователем, комментариями и тегами статьи.
/// </summary>
public class Article
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; } = null!;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }

    public string UserId { get; set; } = null!;

    // Связи
    public virtual User? User { get; set; }

    public virtual ICollection<Tag>? Tags { get; set; }
    public virtual ICollection<Comment>? Comments { get; set; }
}