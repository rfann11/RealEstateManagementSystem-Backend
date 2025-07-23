namespace REMS.Backend.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } //Kullanıcı adı olarak kullanacağım
        public byte[] PasswordHash { get; set; } // Hashlenmiş şifre (SHA-256)
        public byte[] PasswordSalt { get; set; } // Tuzlanmış şifre
        public string Role { get; set; } 


        public ICollection<Tasinmaz> Tasinmazlar { get; set; } = new List<Tasinmaz>(); // Kullanıcının oluşturduğu taşınmaz listesi
        public ICollection<Log> Loglar { get; set; } = new List<Log>(); // Kullanıcının loglarının tutulacağı yer


    }
}
