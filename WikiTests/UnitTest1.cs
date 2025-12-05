using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;

namespace WikiTests
{
    public class Tests
    {
 
        private HttpClient _client;

        [Test]
        public async Task GetWikiPage_Returns200_WhenPageExists()
        {
            var response = await _client.GetAsync("/wiki/existing-page");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetWikiPage_Returns404_WhenPageDoesNotExist()
        {
            var response = await _client.GetAsync("/wiki/nonexistent-page");

            Assert.are(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task GetWikiPage_Returns400_WhenRequestIsBad()
        {
            var response = await _client.GetAsync("/wiki/???invalid-url???");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
