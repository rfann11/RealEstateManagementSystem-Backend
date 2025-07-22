using Microsoft.EntityFrameworkCore;
using REMS.Backend.Entities; 

namespace REMS.Backend.Data 
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public ApplicationDbContext()
        {
        }


        public DbSet<Il> Iller { get; set; }
        public DbSet<Ilce> Ilceler { get; set; }
        public DbSet<Mahalle> Mahalleler { get; set; }
        public DbSet<Tasinmaz> Tasinmazlar { get; set; }
        // Diğer entityler (User, Log vb.) de buraya gelecek


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Bağlantı dizenizi buraya doğrudan yazmayın,
                // gerçek uygulamada appsettings.json'dan alınır.
                // Bu sadece dotnet ef araçlarının DbContext'i bulabilmesi için bir yedek yoldur.
                // Host=localhost;Port=5432;Database=RealEstateDB;Username=remsuser;Password=rems_password
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=RealEstateDB;Username=remsuser;Password=rems_password");
            }
        }
    }
}