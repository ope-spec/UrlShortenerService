using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Models;

namespace UrlShortenerService.Controllers
{
    /// <summary>
    /// Controller to handle URL shortening and redirection.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UrlShortenerController : ControllerBase
    {
        private readonly UrlShortenerContext _context;

        /// <summary>
        /// Initializes a new instance of the UrlShortenerController with the given context.
        /// </summary>
        /// <param name="context">The database context for accessing URL mappings.</param>
        public UrlShortenerController(UrlShortenerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a shortened URL for the given original URL.
        /// </summary>
        /// <param name="request">The request object containing the original URL to shorten.</param>
        /// <returns>The shortened URL if successful; otherwise, an error response.</returns>
        /// <remarks>
        /// If the original URL already has a corresponding short URL, this endpoint returns the existing short URL.
        /// Otherwise, a new short URL is generated, saved to the database, and returned.
        /// </remarks>
        [HttpPost("shorten")]
        public async Task<ActionResult<string>> ShortenUrl([FromBody] UrlShortenRequest request)
        {
            // Check if a short URL already exists for the given original URL
            var existingMapping = await _context.UrlMapping
                .FirstOrDefaultAsync(m => m.OriginalUrl == request.OriginalUrl);
            if (existingMapping != null)
            {
                return Ok($"https://localhost:44365/api/UrlShortener/{existingMapping.ShortUrl}");
            }

            // Generate a unique short URL
            string shortUrl = GenerateShortUrl();
            while (await _context.UrlMapping.AnyAsync(m => m.ShortUrl == shortUrl))
            {
                shortUrl = GenerateShortUrl();
            }

            // Create a new URL mapping and save to database
            var urlMapping = new UrlMapping { OriginalUrl = request.OriginalUrl, ShortUrl = shortUrl };
            _context.UrlMapping.Add(urlMapping);
            await _context.SaveChangesAsync();

            return Ok($"https://localhost:44365/api/UrlShortener/{shortUrl}");
        }

        /// <summary>
        /// Redirects to the original URL associated with the given short URL.
        /// </summary>
        /// <param name="shortUrl">The shortened URL code.</param>
        /// <returns>A redirection to the original URL if found; otherwise, a 404 error.</returns>
        /// <remarks>
        /// This endpoint searches for a mapping that matches the provided short URL. If found, it redirects
        /// the user to the original URL. If not found, it returns a 404 response.
        /// </remarks>
        [HttpGet("{shortUrl}")]
        public async Task<IActionResult> RedirectToUrl(string shortUrl)
        {
            // Find the original URL corresponding to the short URL
            var urlMapping = await _context.UrlMapping
                .FirstOrDefaultAsync(m => m.ShortUrl == shortUrl);
            if (urlMapping != null)
            {
                return Redirect(urlMapping.OriginalUrl);
            }
            return NotFound(new { error = "URL not found!" });
        }

        /// <summary>
        /// Generates a random 6-character short URL code.
        /// </summary>
        /// <returns>A 6-character string representing the short URL.</returns>
        /// <remarks>
        /// This method uses a random selection of alphanumeric characters to create a short URL code. 
        /// It checks for uniqueness within the `ShortenUrl` method to avoid collisions.
        /// </remarks>
        private string GenerateShortUrl()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var shortUrl = new char[6];
            for (int i = 0; i < shortUrl.Length; i++)
            {
                shortUrl[i] = chars[random.Next(chars.Length)];
            }
            return new string(shortUrl);
        }
    }
}
