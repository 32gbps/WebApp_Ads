using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace WebApp_Ads.Controllers
{
    public struct AdPlatform
    {
        public string Name { get; set; }
        public List<string> Locations { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AdPlatformController : ControllerBase
    {
        private static ConcurrentDictionary<string, List<string>> _adPlatforms =
            new ConcurrentDictionary<string, List<string>>();
        [HttpPost("upload")]
        public IActionResult UploadAdPlatforms([FromBody] string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return BadRequest("Invalid file path.");
            }

            try
            {
                // Чтение строк из файла
                var lines = System.IO.File.ReadAllLines(filePath);
                var adPlatforms = new List<AdPlatform>();

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length != 2) continue;

                    var name = parts[0].Trim();
                    var locations = parts[1].Trim().Split(',').Select(l => l.Trim()).ToList();

                    adPlatforms.Add(new AdPlatform { Name = name, Locations = locations });
                }

                // Полностью перезаписываем коллекцию
                _adPlatforms.Clear(); // Удаляем существующие данные
                foreach (var platform in adPlatforms)
                {
                    // Добавляем в коллекцию
                    _adPlatforms.TryAdd(platform.Name, platform.Locations);
                }

                return Ok($"Ad platforms {_adPlatforms.Count} uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public IActionResult SearchAdPlatforms(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                return BadRequest("Location cannot be empty.");
            }

            var result = _adPlatforms.Where(p => p.Value.Any(loc => location.StartsWith(loc))).Select(p => p.Key).ToList();

            return Ok(result);
        }
    }
}
