namespace REMS.Backend.Entities
{
    public class Ilce
    {
        public int Id { get; set; }
        public string Ad { get; set; }

        //Foreign Key
        public int IlId { get; set; }
        
        // Navigasyon özelliği
        public Il Il { get; set; }

        public ICollection<Mahalle> Mahalleler { get; set; }

    }
}
