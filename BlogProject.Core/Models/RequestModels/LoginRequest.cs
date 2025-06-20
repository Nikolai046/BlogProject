﻿using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.RequestModels
{
    public class LoginRequest
    {
        [EmailAddress(ErrorMessage = "Введите правильный адрес электронной почты.")]
        [Required(ErrorMessage = "Поле не может быть пустым.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле не может быть пустым.")]
        [MinLength(6, ErrorMessage = "Минимальная длина пароля — 6 символов.")]
        public string Password { get; set; }
    }
}