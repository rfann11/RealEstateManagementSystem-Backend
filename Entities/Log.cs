namespace REMS.Backend.Entities
{
    public class Log
    {
        public int Id { get; set; }
        public int? UserId { get; set; } //Hangi kullanıcı ?
        public string OperationType { get; set; } // Hangi işlem ?
        public string Description { get; set; } // İşlem detayı
        public DateTime Timestamp { get; set; } // İşlem zamanı
        public string UserIp { get; set; } // Ip adresi
        public string Status { get; set; } // Başarılı/Başarısız

        public User User { get; set; } // Navigasyon
    }
}
