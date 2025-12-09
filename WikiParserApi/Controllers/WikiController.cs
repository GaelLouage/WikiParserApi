using HtmlAgilityPack;
using Infra.Dtos;
using Infra.Enums;
using Infra.Helpers;
using Infra.Interfaces;
using Infra.Models;
using Infra.Services.Classes;
using Infra.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection.Emit;


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
        private readonly IAppMetricsService _appMetricsService;
        public WikiController(
            IWikiParserService parser,
            ILogger<WikiController> logger,
            IMemoryCacheService memoryCacheService,
            IPdfService pdfService,
            IAppMetricsService appMetricsService)
        {
            _parser = parser;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _pdfService = pdfService;
            _appMetricsService = appMetricsService;
        }

        //get full page 
        [HttpPost("parse")]
        public async Task<IActionResult> ParseFullPage(List<WikiRequestEntity> wikiRequests)
        {
            try
            {
                (List<WikiDto> wikiDtosList, string value) = await WikiRequestsAsync(wikiRequests);

                if (!string.IsNullOrEmpty(value))
                {
                    return BadRequest(value);
                }

                return Ok(wikiDtosList);
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"Error parsing" +
                    $"ERROR: {ex.Message} " +
                    $"at {DateTime.UtcNow}";

                return Problem(errorMessage);
            }
        }

        private async Task<(List<WikiDto> wikiDtos, string value)> WikiRequestsAsync(List<WikiRequestEntity> wikiRequests)
        {
            var wikiDtos = new List<WikiDto>();
            WikiDto? wikiDto = new WikiDto();
            foreach (var request in wikiRequests)
            {

                request.Topic = request.Topic?.Trim();
                request.Topic = request.Topic?.Replace(" ", "_");
                if (string.IsNullOrWhiteSpace(request.Topic))
                {
                    return (null, "One of the topics in the request list is null or empty.");
                }

                // get from the cache
                var safeTopic = Uri.EscapeDataString(request.Topic);
                var cacheKey = $"{safeTopic}-fullpage-{request.Language}";
                bool flowControl = await GetWikiFromCache(wikiDtos, cacheKey);
                if (!flowControl)
                {
                    continue;
                }

                (WikiEntity? wikiEntity, List<string> Errors) = await _parser.ExtractPageAsync(request.Topic, request.Language);
                LogInformation(request.Topic, cacheKey, wikiEntity);
                var pdfRequest = (await _pdfService.GeneratePdfFromWikiEntityAsync(wikiEntity));
                wikiDtos.Add(new WikiDto()
                {
                    Errors = Errors,
                    PdfByte64String = pdfRequest.PdfByte64String
                });
            }

            return (wikiDtos, string.Empty);
        }

        private async Task<bool> GetWikiFromCache(List<WikiDto> wikiDtos, string cacheKey)
        {
            if (_memoryCacheService.GetCacheValue(cacheKey, _appMetricsService) is WikiEntity cached)
            {
                var wikiDataFromCache = await _pdfService.GeneratePdfFromWikiEntityAsync(cached);
                wikiDtos.Add(new WikiDto()
                {
                    Errors = wikiDataFromCache.Errors,
                    PdfByte64String = wikiDataFromCache.PdfByte64String
                });
                return false;
            }

            return true;
        }

        private void LogInformation(string topic, string cacheKey, WikiEntity wikiEntity)
        {
            _logger.LogInformation(
                $"Wiki Endpoint called at \n" +
                $"TimeStamp: {DateTime.UtcNow} \n" +
                $"IP: {HostHelpers.GetLocalIPAddress()}");

            _memoryCacheService.SetCacheValue(
                cacheKey,
                wikiEntity,
                TimeSpan.FromSeconds(30));
            _logger.LogInformation(
                $"{topic} - full page cached in memory \n" +
                $"{DateTime.UtcNow}");
        }
    }
}
