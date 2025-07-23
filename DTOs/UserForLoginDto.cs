using System.ComponentModel.DataAnnotations;

namespace REMS.Backend.DTOs
{
    public class UserForLoginDto
    {
        [Required(ErrorMessage = "Email alanı zorunludur !")]
        [EmailAddress(ErrorMessage = "Geçerli bir email girin !")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur !")]
        public string Password { get; set; }

    }
}
