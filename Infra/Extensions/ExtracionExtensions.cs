using HtmlAgilityPack;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Infra.Extensions
{
    public static class ExtracionExtensions
    {
        // extract image url from the page
        public static List<string> ExtractImagesUrl(this HtmlDocument doc)
        {
            var contentDiv = doc.DocumentNode
                                .SelectSingleNode("//div[@id='mw-content-text']//div[contains(@class,'mw-parser-output')]");
            if (contentDiv == null) return new List<string>();

            var images = contentDiv.SelectNodes(".//img")
                ?.Select(img => img.GetAttributeValue("src", null))
                .Where(src => !string.IsNullOrWhiteSpace(src))
                .Select(src => src.StartsWith("http") ? src : $"https:{src}") // make URLs absolute
                .ToList() ?? new List<string>();

            return images;
        }


        public static List<string> ExtractMultipleParagraphs(this HtmlDocument doc, int paragraphCount = 1)
        {
            var contentDiv = doc.DocumentNode
                                .SelectSingleNode("//div[@id='mw-content-text']//div[contains(@class,'mw-parser-output')]");
            if (contentDiv == null) return new List<string>();

            var paragraphs = contentDiv.SelectNodes(".//p")
                ?.Where(p =>
                {
                    var text = p.InnerText.Trim();

                    // Skip empty or very short paragraphs
                    if (string.IsNullOrWhiteSpace(text) || text.Length < 50)
                        return false;

                    // Skip common banner/quality label keywords in any language
                    var lower = text.ToLower();
                    var skipKeywords = new[] { "bon article", "excellente", "à améliorer", "exzellenter artikel" };
                    if (skipKeywords.Any(k => lower.Contains(k)))
                        return false;

                    return true;
                })
                .Select(p => p.InnerText.Trim())
                .Take(paragraphCount)
                .ToList() ?? new List<string>();

            return paragraphs;
        }

        public static List<string> ExtractFullContent(this HtmlDocument doc) =>
             doc.DocumentNode
                     .SelectNodes("//p[not(@class)]")
                     .Select(x => x.InnerText.Trim())
                     .ToList() ?? new List<string>();


        // get title of the article
        public static string ExtractTitle(this HtmlDocument doc) =>
            doc.DocumentNode
                .SelectSingleNode("//h1[@id='firstHeading']")?
                .InnerText
                .Trim() ??
                "No title found.";
    }
}
