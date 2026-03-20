using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Flux.Infrastructure.Client;

public abstract class BaseClientService
{
    protected readonly HttpClient HttpClient;
    protected readonly IJSRuntime JsRuntime;

    protected BaseClientService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        HttpClient = httpClient;
        JsRuntime = jsRuntime;
    }

    protected async Task SetAuthHeaderAsync()
    {
        try
        {
            var token = await JsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                token = await JsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // Ignore JS interop errors
        }
    }
}
