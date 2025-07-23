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
        public DbSet<User> Kullanıcılar { get; set; }
        public DbSet<Log> Loglar { get; set; }

        // ilişkiler
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // il-ilçe
            modelBuilder.Entity<Ilce>()
               .HasOne(ilce => ilce.Il)        // bir ilçe bir ile sahip
               .WithMany(il => il.Ilceler)     // bir il çok ilçeli
               .HasForeignKey(ilce => ilce.IlId); // İlId, İlçe'deki Foreign Key'dir

            // ilçe - mahalle
            modelBuilder.Entity<Mahalle>()
                .HasOne(mahalle => mahalle.Ilce)
                .WithMany(ilce => ilce.Mahalleler)
                .HasForeignKey(mahalle => mahalle.IlceId);

            // mahalle - tasinmaz 
            modelBuilder.Entity<Tasinmaz>()
                .HasOne(tasinmaz => tasinmaz.Mahalle)
                .WithMany(mahalle => mahalle.Tasinmazlar)
                .HasForeignKey(tasinmaz => tasinmaz.MahalleId);

            // kullanıcı - tasinmaz 
            modelBuilder.Entity<Tasinmaz>()
                .HasOne(tasinmaz => tasinmaz.User)
                .WithMany(user => user.Tasinmazlar)
                .HasForeignKey(tasinmaz => tasinmaz.UserId)
                .OnDelete(DeleteBehavior.SetNull); //OnDelete(yapay zeka) | Kullanıcı silindiğinde taşınmazlarınıda silecek

            // Kullanıcı - Loglar
            modelBuilder.Entity<Log>()
                .HasOne(log => log.User)
                .WithMany(user => user.Loglar)
                .HasForeignKey(log => log.UserId)
                .IsRequired(false) //UserId nullable olduğu için zorunlu değil
                .OnDelete(DeleteBehavior.SetNull);

            // Email benzersiz olsun | Yapay Zeka
            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique(); 
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=RealEstateDB;Username=remsuser;Password=rems_password");
            }
        }
    }
}