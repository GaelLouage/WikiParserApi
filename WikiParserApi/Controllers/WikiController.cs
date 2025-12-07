using Infra.Dtos;
using Infra.Enums;
using Infra.Interfaces;
using Infra.Models;
using Infra.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.RateLimiting;


namespace WikiParserApi.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/rest_v1/[controller]")]
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
        [HttpGet("{topic}/{language}")]
        public async Task<IActionResult> ParseFullPage(string topic,LanguageType language)
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
                var cacheKey = $"{safeTopic}-fullpage-{language}";
                (bool flowControl, IActionResult value) = await CheckCacheAsync(cacheKey);
                if (!flowControl)
                {
                    return value;
                }
                (WikiEntity? wikiEntity, List<string> Errors) = await _parser.ExtractPageAsync(topic, language);
                LogInformation(topic, cacheKey, wikiEntity);

                wikiDto = await _pdfService.GeneratePdfFromWikiEntityAsync(wikiEntity);
                wikiDto.Errors.AddRange(Errors);

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

        private void LogInformation(string topic, string cacheKey, WikiEntity wikiEntity)
        {
            _logger.LogInformation(
                $"Wiki Endpoint called at " +
                $"{DateTime.UtcNow}");

            _memoryCacheService.SetCacheValue(
                cacheKey,
                wikiEntity,
                TimeSpan.FromSeconds(30));
            _logger.LogInformation(
                $"{topic} - full page cached in memory " +
                $"{DateTime.UtcNow}");
        }

        private async Task<(bool flowControl, IActionResult value)> CheckCacheAsync(string cacheKey)
        {
            if (_memoryCacheService.GetCacheValue(cacheKey) is WikiEntity cached)
            {
                var cachedToPdfBase64 = await _pdfService.GeneratePdfFromWikiEntityAsync(cached);
                return (flowControl: false, value: Ok(cachedToPdfBase64));
            }

            return (flowControl: true, value: null);
        }
    }
}
