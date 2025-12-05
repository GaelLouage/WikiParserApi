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
            var imageNode = doc.DocumentNode
                .SelectNodes("//table[contains(@class, 'infobox')]//img");

            // get all images url
            return imageNode?
                .Select(img => img.GetAttributeValue("src", null))
                .Where(src => src != null)
                .ToList() ?? new List<string>();
        }
        public  static List<string> ExtractMultipleParagraphs(this HtmlDocument doc, int paragraphCount = 1) =>
            doc.DocumentNode
                    .SelectNodes("//p[not(@class)]")
                    ?.Take(paragraphCount)
                    .Select(x => x.InnerText.Trim())
                    .ToList() ?? new List<string>();

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
