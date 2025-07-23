using System.ComponentModel.DataAnnotations;

namespace REMS.Backend.DTOs
{
    public class UserForUpdateDto
    {
        [Required(ErrorMessage = " Id alanı zorunlu ")]
        public int Id { get; set; }

        [Required(ErrorMessage = " Email alanı zorunlu ")]
        [EmailAddress(ErrorMessage = " Geçerli bir email adresi girin ")]
        public string Email { get; set; }

        [StringLength(12, MinimumLength = 8, ErrorMessage = "Şifre 8 ile 12 karakter arasında olmalıdır.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = " Rol alanı zorunlu ")]
        [RegularExpression("^(Admin|User)$", ErrorMessage = "Rol 'Admin' veya 'User' olmalıdır.")]
        public string Role { get; set; }    


    }
}
