using HtmlAgilityPack;
using Infra.Classes;
using Infra.Enums;
using Infra.Extensions;
using Infra.Helpers;
using Infra.Interfaces;
using Infra.Models;
using Infra.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Wrap;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Services.Classes
{
    public class WikiParserService : IWikiParserService
    {
        private readonly AsyncPolicyWrap _policy;
        private HtmlWeb _web;
        private readonly IConfiguration _config;

        public WikiParserService(IConfiguration config)
        {

            _web = new HtmlWeb();
            _policy = WikiPoliciesHelpers.CreateRetryTimeoutPolicy();
            _config = config;
        }

        // make page with title and paragraphs
        public async Task<(WikiEntity, List<string> Errors)> ExtractPageAsync(
            string topic,
             LanguageType language =
            LanguageType.English,
            bool fullContent = false,
            int paragraphCount = 1
           )
        {
            var errors = new List<string>();
            try
            {

                string? wikiBaseUrl = SetWikiLanguage(language);

                return (await _policy.ExecuteAsync(async () =>
                {
                    var url = $"{wikiBaseUrl}{topic}";
                    var doc = await _web.LoadFromWebAsync(url);

                    var title = doc.ExtractTitle();
                    var body = GetBody(fullContent, paragraphCount, doc);
                    var images = doc.ExtractImagesUrl();


                    errors = CheckPageValidation(title, body, images);
                 

                    return new WikiEntity
                    {
                        Topic = topic,
                        Title = title,
                        Body = body,
                        ImageUrl = images
                    };

                }), errors);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string? SetWikiLanguage(LanguageType language)
        {
            var wikiBaseUrls = _config.GetSection("WikiBaseUrl");
            return language switch
            {
                LanguageType.Dutch => wikiBaseUrls.GetSection("nl").Value,
                LanguageType.Spanish => wikiBaseUrls.GetSection("es").Value,
                LanguageType.French => wikiBaseUrls.GetSection("fr").Value,
                LanguageType.German => wikiBaseUrls.GetSection("de").Value,
                _ => wikiBaseUrls.GetSection("en").Value,
            };
        }

        private static List<string> CheckPageValidation(string title, List<string> body, List<string> images)
        {
            var errors = new List<string>();    
            //  error if not title
            if (string.IsNullOrEmpty(title))
            {
                errors.Add("Title not found.");
            }
            //error if image not found
            if (images.Count == 0)
            {
                errors.Add("Images not found.");
            }
            // error if body is empty
            if (body.Count == 0)
            {
                errors.Add("Body not found.");
            }
            return errors;
        }

        private static List<string> GetBody(bool fullContent, int paragraphCount, HtmlDocument doc)
        {
            List<string> body;
            if (fullContent)
            {
                body = doc.ExtractFullContent();
            }
            else
            {
                //error if paragraph count less than 1
                if (paragraphCount < 1)
                {
                    throw new Exception("Paragraph count must be at least 1.");
                }
                body = doc.ExtractMultipleParagraphs(paragraphCount);
            }

            return body;
        }
    }
}
