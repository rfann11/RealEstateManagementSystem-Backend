namespace REMS.Backend.Entities
{
    public class Tasinmaz
    {
        public int Id { get; set; }
        public string Adres { get; set; }
        public string ParselNo { get; set; }
        public string AdaNo { get; set; }
        public string Koordinat { get; set; }
        public string TasinmazTipi { get; set; }

        //Foreign Key
        public int MahalleId { get; set; }
        public int UserId { get; set; }

        // Navigasyon özelliği
        public Mahalle Mahalle { get; set; }
        public User User { get; set; }




    }
}
