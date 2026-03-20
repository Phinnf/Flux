using System.Net.Http.Json;
using System.Text.Json;
using Flux.Domain.Common;
using Microsoft.JSInterop;

namespace Flux.Infrastructure.Client;

public class UploadClientService : BaseClientService
{
    public UploadClientService(HttpClient httpClient, IJSRuntime jsRuntime) : base(httpClient, jsRuntime) { }

    public async Task<Result<string>> UploadImageAsync(MultipartFormDataContent content)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsync("/api/uploads/image", content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return Result<string>.CreateSuccess(result.GetProperty("url").GetString()!);
            }
            var error = await response.Content.ReadAsStringAsync();
            return Result<string>.CreateFailure(error);
        }
        catch (Exception ex)
        {
            return Result<string>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<string>> UploadAudioAsync(MultipartFormDataContent content)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsync("/api/uploads/audio", content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return Result<string>.CreateSuccess(result.GetProperty("url").GetString()!);
            }
            var error = await response.Content.ReadAsStringAsync();
            return Result<string>.CreateFailure(error);
        }
        catch (Exception ex)
        {
            return Result<string>.CreateFailure(ex.Message);
        }
    }
}
