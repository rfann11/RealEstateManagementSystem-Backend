namespace REMS.Backend.Entities
{
    public class Mahalle
    {
        public int Id { get; set; }
        public string Ad { get; set; }

        // Foreign Key
        public int IlceId { get; set; }
        // Navigasyon özelliği
        public Ilce Ilce { get; set; }

        // İlişkisel özellikler
        public ICollection<Tasinmaz> Tasinmazlar { get; set; }
    }
}
