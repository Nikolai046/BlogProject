﻿using System.ComponentModel.DataAnnotations;

namespace BlogProject.Data.Entities;

public class Tag
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    // Связь многие-ко-многим
    public ICollection<Article> Articles { get; set; }
}