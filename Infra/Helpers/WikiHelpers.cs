using Infra.Dtos;
using Infra.Interfaces;
using Infra.Models;
using Infra.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Helpers
{
    public static class WikiHelpers
    {
        public static async Task<(List<WikiDto> wikiDtos, string value)> WikiRequestsAsync<T>(
            IWikiParserService parser,
            IPdfService pdfService,
            ILogger<T> logger,
            IMemoryCacheService memoryCacheService,
            IAppMetricsService appMetricsService,
            List<WikiRequestEntity> wikiRequests)
        {
            var wikiDtos = new List<WikiDto>();
            WikiDto? wikiDto = new WikiDto();
            (bool flowControl, (List<WikiDto> wikiDtos, string value) value) = 
                await PopulateWikiDto(parser,
                pdfService, 
                logger,
                memoryCacheService,
                appMetricsService,
                wikiRequests,
                wikiDtos);

            if (!flowControl)
            {
                return value;
            }

            return (wikiDtos, string.Empty);
        }

        private static async Task<(bool flowControl, (List<WikiDto> wikiDtos, string value) value)> PopulateWikiDto<T>(IWikiParserService parser, IPdfService pdfService, ILogger<T> logger, IMemoryCacheService memoryCacheService, IAppMetricsService appMetricsService, List<WikiRequestEntity> wikiRequests, List<WikiDto> wikiDtos)
        {
            foreach (var request in wikiRequests)
            {

                request.Topic = request.Topic?.Trim();
                request.Topic = request.Topic?.Replace(" ", "_");
                if (string.IsNullOrWhiteSpace(request.Topic))
                {
                    return (flowControl: false, value: (null, "One of the topics in the request list is null or empty."));
                }

                // get from the cache
                var safeTopic = Uri.EscapeDataString(request.Topic);
                var cacheKey = $"{safeTopic}-fullpage-{request.Language}";
                bool flowControl = await GetWikiFromCache(memoryCacheService, appMetricsService, pdfService, wikiDtos, cacheKey);
                if (!flowControl)
                {
                    continue;
                }

                (WikiEntity? wikiEntity, List<string> Errors) = await parser.ExtractPageAsync(request.Topic, request.Language);
                LogInformation(logger, memoryCacheService, request.Topic, cacheKey, wikiEntity);
                var pdfRequest = (await pdfService.GeneratePdfFromWikiEntityAsync(wikiEntity));
                wikiDtos.Add(new WikiDto()
                {
                    Errors = Errors,
                    PdfByte64String = pdfRequest.PdfByte64String
                });
            }

            return (flowControl: true, value: default);
        }

        private static async Task<bool> GetWikiFromCache(
            IMemoryCacheService memoryCacheService,
            IAppMetricsService appMetricsService,
            IPdfService pdfService,
            List<WikiDto> wikiDtos,
            string cacheKey)
        {
            if (memoryCacheService.GetCacheValue(cacheKey, appMetricsService) is WikiEntity cached)
            {
                var wikiDataFromCache = await pdfService.GeneratePdfFromWikiEntityAsync(cached);
                wikiDtos.Add(new WikiDto()
                {
                    Errors = wikiDataFromCache.Errors,
                    PdfByte64String = wikiDataFromCache.PdfByte64String
                });
                return false;
            }

            return true;
        }

        private static void LogInformation<T>(
            ILogger<T> logger,
            IMemoryCacheService memoryCacheService, 
            string topic, 
            string cacheKey, 
            WikiEntity wikiEntity)
        {
            logger.LogInformation(
                $"Wiki Endpoint called at \n" +
                $"TimeStamp: {DateTime.UtcNow} \n" +
                $"IP: {HostHelpers.GetLocalIPAddress()}");

            memoryCacheService.SetCacheValue(
                cacheKey,
                wikiEntity,
                TimeSpan.FromSeconds(30));
            logger.LogInformation(
                $"{topic} - full page cached in memory \n" +
                $"{DateTime.UtcNow}");
        }
    }
}
