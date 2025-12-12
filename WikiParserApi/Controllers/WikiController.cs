using HtmlAgilityPack;
using Infra.Constants;
using Infra.Dtos;
using Infra.Enums;
using Infra.Helpers;
using Infra.Interfaces;
using Infra.Models;
using Infra.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection.Emit;


namespace WikiParserApi.Controllers
{
    [EnableRateLimiting("fixed")]
    [Authorize(Roles = $"{RoleType.SuperAdmin},{RoleType.Admin},{RoleType.User}")]
    [ApiController]
    [Route("api/rest_v1/[controller]")]
    public class WikiController : ControllerBase
    {
        private readonly IWikiParserService _parser;
        private readonly IPdfService _pdfService;
        private readonly ILogger<WikiController> _logger;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IAppMetricsService _appMetricsService;
        private readonly IJwtTokenService _jwtTokenService;
        public WikiController(
            IWikiParserService parser,
            ILogger<WikiController> logger,
            IMemoryCacheService memoryCacheService,
            IPdfService pdfService,
            IAppMetricsService appMetricsService,
            IJwtTokenService jwtTokenService)
        {
            _parser = parser;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _pdfService = pdfService;
            _appMetricsService = appMetricsService;
            _jwtTokenService = jwtTokenService;
        }

        //get full page 
        [HttpPost("parse")]
        public async Task<IActionResult> ParseFullPage(List<WikiRequestEntity> wikiRequests)
        {
            try
            {
                (List<WikiDto> wikiDtosList, string value) = await WikiHelpers.WikiRequestsAsync<WikiController>(
                    _parser,
                    _pdfService,
                    _logger,
                    _memoryCacheService,
                    _appMetricsService,
                    wikiRequests);

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
    }
}
