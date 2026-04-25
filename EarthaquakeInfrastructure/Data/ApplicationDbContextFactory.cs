using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EarthaquakeInfrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Buraya connection stringini koy (docker-compose'da kullanılan connection string ile aynı olmalı)
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=EarthquakeDb;User Id=sa;Password=123123Fburak.;TrustServerCertificate=True");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
