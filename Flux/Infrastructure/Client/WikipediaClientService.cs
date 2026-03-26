using System.Net.Http.Json;
using Flux.Domain.Common;
using Microsoft.JSInterop;

namespace Flux.Infrastructure.Client;

public record WikiResult(string Title, string Snippet, string Url);

public class WikipediaClientService : BaseClientService
{
    public WikipediaClientService(HttpClient httpClient, IJSRuntime jsRuntime) : base(httpClient, jsRuntime) { }

    public async Task<Result<List<WikiResult>>> SearchAsync(string query)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.GetFromJsonAsync<Result<List<WikiResult>>>($"/api/wikipedia/search?query={query}");
            return response ?? Result<List<WikiResult>>.CreateFailure("Failed to search Wikipedia.");
        }
        catch (Exception ex)
        {
            return Result<List<WikiResult>>.CreateFailure(ex.Message);
        }
    }
}
