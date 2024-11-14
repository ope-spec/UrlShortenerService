using Microsoft.EntityFrameworkCore;

namespace UrlShortenerService.Models
{
    public class UrlShortenerContext:DbContext
    {
        public UrlShortenerContext(DbContextOptions<UrlShortenerContext> options) : base(options) { }

        public DbSet<UrlMapping> UrlMapping { get; set; }
    }
}
