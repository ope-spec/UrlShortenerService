using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace UrlShortenerService.Models
{
    public class UrlShortenerContextFactory : IDesignTimeDbContextFactory<UrlShortenerContext>
    {
        public UrlShortenerContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UrlShortenerContext>();

            // Load configuration from appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new UrlShortenerContext(optionsBuilder.Options);
        }
    }
}
