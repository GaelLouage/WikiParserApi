using HtmlAgilityPack;
using Infra.Dtos;
using Infra.Models;
using Infra.Services.Interfaces;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using System;
using System.IO;
using System.Threading;

public class PdfService : IPdfService
{
    public async Task<WikiDto?> GeneratePdfFromWikiEntityAsync(WikiEntity wikiEntity)
    {
        var wikiDto = new WikiDto();
        if (wikiEntity == null) throw new ArgumentNullException(nameof(wikiEntity));

        // 1. Create a new MigraDoc document
        var document = new Document();
        document.Info.Title = wikiEntity.Title ?? "Untitled";
        document.Info.Subject = wikiEntity.Topic ?? "No Topic";
        // 2. Add a section
        var section = document.AddSection();

        // 3. Add Topic
        var topicParagraph = section.AddParagraph($"Topic: {wikiEntity.Topic}");
        topicParagraph.Format.Font.Name = "Arial"; // guaranteed safe font
        topicParagraph.Format.Font.Size = 14;
        topicParagraph.Format.SpaceAfter = 6;

        // 4. Add Title
        var titleParagraph = section.AddParagraph($"Title: {wikiEntity.Title}");
        titleParagraph.Format.Font.Name = "Arial";
        titleParagraph.Format.Font.Size = 12;
        titleParagraph.Format.SpaceAfter = 12;

        // 5. Add body paragraphs
        if (wikiEntity.Body != null)
        {
            foreach (var paragraphText in wikiEntity.Body)
            {
                var paragraph = section.AddParagraph(paragraphText);
                paragraph.Format.Font.Name = "Arial";
                paragraph.Format.Font.Size = 10;
                paragraph.Format.SpaceAfter = 6;
            }
        }

        if (wikiEntity.ImageUrl != null)
        {
            using var http = new HttpClient();

            // Pretend to be a browser
            http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

            foreach (var imageUrl in wikiEntity.ImageUrl)
            {
                try
                {
                    var bytes = await http.GetByteArrayAsync(imageUrl);

                   // saved location = C: \Users\< YourUser >\AppData\Local\Temp\
                    string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
                    await File.WriteAllBytesAsync(tempFile, bytes);

                    var image = section.AddImage(tempFile);
                    image.Width = "5cm";
                    image.LockAspectRatio = true;

                    section.AddParagraph();
                }
                catch (Exception ex)
                {
                    wikiDto.Errors.Add($"Failed to load image from URL: {imageUrl}\nERROR: {ex.Message}");
                }
            }
        }

        // 7. Render document to PDF
        var pdfRenderer = new PdfDocumentRenderer() { Document = document };
        
        pdfRenderer.RenderDocument();

        // 8. Save PDF to memory and convert to Base64
        using (var ms = new MemoryStream())
        {
            pdfRenderer.PdfDocument.Save(ms, false);
            var pdfBytes = ms.ToArray();
            var pdfByte = Convert.ToBase64String(pdfBytes);
            wikiDto.PdfByte64String = pdfByte;
            return wikiDto;
        }
    }
}
