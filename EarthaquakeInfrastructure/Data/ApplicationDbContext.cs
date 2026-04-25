using EarthaquakeApplication.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace EarthaquakeInfrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<EarthquakeModel> Earthquakes { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<FamilyMember>()
               .HasOne(fm => fm.ApplicationUser) // Bir FamilyMember'ın bir ApplicationUser'ı vardır
               .WithMany() // Bir ApplicationUser'ın birden çok FamilyMember'ı olabilir.
               .HasForeignKey(fm => fm.ApplicationUserId) // ApplicationUserId Foreign Key'dir
               .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde aile üyeleri de silinsin
        }
    }
}
