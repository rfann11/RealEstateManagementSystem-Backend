using System.ComponentModel.DataAnnotations;


namespace REMS.Backend.DTOs
{
    public class UserForRegisterDto
    {
        [Required(ErrorMessage = "Email alanı zorunlu")]
        [EmailAddress(ErrorMessage ="Geçerli bir email adresi giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunlu")]
        [StringLength(12, MinimumLength = 8, ErrorMessage = "Geçerli bir şifre girin")]
        public string Password { get; set; }
    }
}
