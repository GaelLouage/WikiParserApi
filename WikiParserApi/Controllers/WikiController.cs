using Infra.Interfaces;
using Infra.Models;
using Infra.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WikiParserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WikiController : ControllerBase
    {
        private readonly IWikiParserService _parser;
        private readonly ILogger<WikiController> _logger;
        private readonly IMemoryCacheService _memoryCacheService;
        public WikiController(IWikiParserService parser, ILogger<WikiController> logger, IMemoryCacheService memoryCacheService)
        {
            _parser = parser;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
        }


        [HttpGet("topic/{topic}")]
        public async Task<IActionResult> Parse(string topic)
        {
            try
            {
                var result = await _parser.ExtractFirstParagraphAsync(topic);
                _logger.LogInformation($"Wiki Endpoint called at {DateTime.UtcNow}");
                var getCacheValue = _memoryCacheService.GetCacheValue(topic);

                if (getCacheValue is not null)
                {
                    return Ok(getCacheValue);
                }

                _memoryCacheService.SetCacheValue(topic, result, TimeSpan.FromSeconds(30));
                _logger.LogInformation($"{topic} cached in memory {DateTime.UtcNow}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Wiki Endpoint called at {DateTime.UtcNow},\n Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
    }
}
