using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Models;


namespace UrlShortenerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrlShortenerController : ControllerBase
    {
        private readonly UrlShortenerContext _context;

        public UrlShortenerController(UrlShortenerContext context)
        {
            _context = context;
        }

        [HttpPost("shorten")]
        public async Task<ActionResult<string>> ShortenUrl([FromBody] UrlShortenRequest request)
        {
            var existingMapping = await _context.UrlMapping
                .FirstOrDefaultAsync(m => m.OriginalUrl == request.OriginalUrl);
            if (existingMapping != null)
            {
                return Ok($"https://localhost:44365/api/UrlShortener/{existingMapping.ShortUrl}");
            }

            string shortUrl = GenerateShortUrl();
            while (await _context.UrlMapping.AnyAsync(m => m.ShortUrl == shortUrl))
            {
                shortUrl = GenerateShortUrl();
            }

            var urlMapping = new UrlMapping { OriginalUrl = request.OriginalUrl, ShortUrl = shortUrl };
            _context.UrlMapping.Add(urlMapping);
            await _context.SaveChangesAsync();

            return Ok($"https://localhost:44365/api/UrlShortener/{shortUrl}");
        }





        [HttpGet("{shortUrl}")]
        public async Task<IActionResult> RedirectToUrl(string shortUrl)
        {
            var urlMapping = await _context.UrlMapping
                .FirstOrDefaultAsync(m => m.ShortUrl == shortUrl);
            if (urlMapping != null)
            {
                return Redirect(urlMapping.OriginalUrl);
            }
            return NotFound(new { error = "URL not found!" });
        }

        // Generate a short URL (same as before)
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

