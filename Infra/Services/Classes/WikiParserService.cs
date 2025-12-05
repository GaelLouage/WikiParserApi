using HtmlAgilityPack;
using Infra.Classes;
using Infra.Extensions;
using Infra.Helpers;
using Infra.Interfaces;
using Infra.Models;
using Infra.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Polly.Wrap;
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
        private readonly string _wikiBaseUrl;
        private readonly AsyncPolicyWrap _policy;
        private HtmlWeb _web;
        public WikiParserService(string baseUrl)
        {
            _wikiBaseUrl = baseUrl;
            _web = new HtmlWeb();
            _policy = WikiPoliciesHelpers.CreateRetryTimeoutPolicy();
        }

        // make page with title and paragraphs
        public async Task<WikiEntity> ExtractPageAsync(string topic, bool fullContent = false, int paragraphCount = 1)
        {
            try
            {
                return await _policy.ExecuteAsync(async () =>
                {
                    var url = $"{_wikiBaseUrl}{topic}";
                    var doc = await _web.LoadFromWebAsync(url);
                    
                    var title = doc.ExtractTitle();
                    var body = GetBody(fullContent, paragraphCount, doc);
                    var images = doc.ExtractImagesUrl();

                    for (int i = 0; i < images.Count; i++)
                    {
                        var imageLink = $"https:{images[i]}";
                        images[i] = imageLink;
                    }


                    (bool IsValid, string Message) = PageIsValid(title, body, images);
                    if(IsValid is false)
                    {
                        throw new Exception(Message);
                    }

                    return new WikiEntity
                    {
                        Topic = topic,
                        Title = title,
                        Body = body,
                        ImageUrl = images
                    };

                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static (bool IsValid, string Message) PageIsValid(string title, List<string> body, List<string> images)
        {
            //  error if not title
            if (string.IsNullOrEmpty(title))
            {
                return (false, "Title not found.");
            }
            //error if image not found
            if (images.Count == 0)
            {
                return (false, "Images not found.");
            }
            // error if body is empty
            if (body.Count == 0)
            {
                return (false, "Body not found.");
            }
            return (true, "Page is valid.");
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
