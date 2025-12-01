using HtmlAgilityPack;
using Infra.Classes;
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
        public WikiParserService(string baseUrl)
        {
            _wikiBaseUrl = baseUrl;
            _policy = WikiPoliciesHelpers.CreateRetryTimeoutPolicy();
        }
        public async Task<WikiEntity> ExtractFirstParagraphAsync(string topic)
        {
            try
            {
                return await _policy.ExecuteAsync(async () =>
                {
                    // From Web
                    var url = $"{_wikiBaseUrl}{topic}";
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(url);

                    return new WikiEntity
                    {
                        Topic = topic,
                        Excerpt = doc.DocumentNode
                    .SelectSingleNode("//p[not(@class)]")?
                    .InnerText
                    .Trim() ??
                    "No paragraph found."
                    };
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
