using HtmlAgilityPack;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static System.Net.WebRequestMethods;

namespace WikiIntegrationTests
{
    public class UnitTest1
    {
        private HttpClient _client = new HttpClient();
        private const string BASE_URL = "https://en.wikipedia.org/wiki/";

        [Fact]
        public async Task GetWikiPage_Returns200_WhenPageExists()
        {
            // From Web
            var url = $"{BASE_URL}Britney";
            var web = new HtmlWeb();

            Assert.Equal(HttpStatusCode.OK, web.StatusCode);
        }

        [Fact]
        public async Task GetWikiPage_Returns403_WhenPageForbidden()
        {
            // From Web

            var response = await _client.GetAsync(BASE_URL);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        }
    }
}
