using Infra.Dtos;
using Infra.Interfaces;
using Infra.Models;
using Infra.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace WikiParserApi.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/[controller]")]
    public class WikiController : ControllerBase
    {
        private readonly IWikiParserService _parser;
        private readonly IPdfService _pdfService;
        private readonly ILogger<WikiController> _logger;
        private readonly IMemoryCacheService _memoryCacheService;
        public WikiController(
            IWikiParserService parser,
            ILogger<WikiController> logger,
            IMemoryCacheService memoryCacheService,
            IPdfService pdfService)
        {
            _parser = parser;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _pdfService = pdfService;
        }

        //get full page 
        [HttpGet("{topic}")]
        public async Task<IActionResult> ParseFullPage(string topic)
        {
            var wikiDto = new WikiDto();
            topic = topic?.Trim();
            if (string.IsNullOrWhiteSpace(topic))
            {
                return BadRequest("Topic cannot be null or empty.");
            }
            try
            {
                var safeTopic = Uri.EscapeDataString(topic);
                var cacheKey = $"{safeTopic}-fullpage";
                if (_memoryCacheService.GetCacheValue(cacheKey) is WikiEntity cached)
                {
                    return Ok(cached);
                }
                var result = await _parser.ExtractPageAsync(topic);
                _logger.LogInformation(
                    $"Wiki Endpoint called at " +
                    $"{DateTime.UtcNow}");
            
                _memoryCacheService.SetCacheValue(
                    cacheKey,
                    result, 
                    TimeSpan.FromSeconds(30));
                _logger.LogInformation(
                    $"{topic} - full page cached in memory " +
                    $"{DateTime.UtcNow}");

                wikiDto = await _pdfService.GeneratePdfFromWikiEntityAsync(result);
                
                return Ok(wikiDto);
            }
            catch (Exception ex)
            {
                var errorMessage = 
                    $"Error parsing full page for topic '{topic}' " +
                    $"ERROR: {ex.Message} " +
                    $"at {DateTime.UtcNow}";

                _logger.LogError(errorMessage);
                wikiDto.Errors.Add(errorMessage);
                return Problem(string.Join("\n",wikiDto.Errors.Select(x => $"{x}")));
            }
        }
    }
}
