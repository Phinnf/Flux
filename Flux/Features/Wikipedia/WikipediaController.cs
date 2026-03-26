using System.Net.Http.Json;
using System.Text.Json;
using Flux.Domain.Common;
using Flux.Infrastructure.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Wikipedia
{
    [Authorize]
    [ApiController]
    [Route("api/wikipedia")]
    public class WikipediaController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public WikipediaController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("search")]
        public async Task<ActionResult<Result<List<WikiResult>>>> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Ok(Result<List<WikiResult>>.CreateSuccess(new List<WikiResult>()));

            try
            {
                var url = $"https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch={Uri.EscapeDataString(query)}&format=json&origin=*";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode) return Ok(Result<List<WikiResult>>.CreateFailure("Wikipedia API error"));

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var searchResults = doc.RootElement.GetProperty("query").GetProperty("search");

                var results = new List<WikiResult>();
                foreach (var item in searchResults.EnumerateArray())
                {
                    var title = item.GetProperty("title").GetString() ?? "";
                    results.Add(new WikiResult(
                        title,
                        item.GetProperty("snippet").GetString() ?? "",
                        $"https://en.wikipedia.org/wiki/{Uri.EscapeDataString(title.Replace(" ", "_"))}"
                    ));
                }

                return Ok(Result<List<WikiResult>>.CreateSuccess(results));
            }
            catch (Exception ex)
            {
                return Ok(Result<List<WikiResult>>.CreateFailure(ex.Message));
            }
        }
    }
}
