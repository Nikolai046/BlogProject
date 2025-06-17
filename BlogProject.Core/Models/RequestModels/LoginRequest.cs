using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.RequestModels
{
    public class LoginRequest
    {

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Пожалуйста, введите корректный email.")]
        public required string Email { get; set; }


        public required string Password { get; set; }
    }
}