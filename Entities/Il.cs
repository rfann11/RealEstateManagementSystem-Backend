namespace REMS.Backend.Entities
{
    public class Il
    {
        public int Id { get; set; }
        public string Ad { get; set; }

        public ICollection<Ilce> Ilceler { get; set; }
    }
}
