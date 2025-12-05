// See https://aka.ms/new-console-template for more information
using Infra.Dtos;
using Infra.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Net.WebRequestMethods;

Console.WriteLine("Hello, World!");

var baseUrl = "https://localhost:7072/api/Wiki/";

HttpClientService example = new HttpClientService();
var wikiData = await example.GetWikiData($"{baseUrl}Britney_Spears");

Console.WriteLine(wikiData.PdfByte64String);

//wikiData.Body.ForEach(paragraph =>
//{
//    Console.WriteLine(paragraph);
//});

Console.ReadLine();
public class HttpClientService
{
    private static readonly HttpClient client = new HttpClient();
    public async Task<WikiDto> GetWikiData(string url)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode == false)
            {
                throw new Exception("Request failed with status: " + response.StatusCode);
            }
            string responseBody = await response.Content.ReadAsStringAsync();

            // make json convertor from text.json serializeaiton
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            //conver responseBody to WikiEntity
            var wikiEntity = JsonSerializer.Deserialize<WikiDto>(responseBody, jsonOptions);

            return wikiEntity;
        }
        catch (Exception ex)
        {

            return new WikiDto();
        }
   
    }
}
