using System.Net.Http.Json;
using Flux.Domain.Common;
using Microsoft.JSInterop;

namespace Flux.Infrastructure.Client;

public record WeatherData(string City, double Temperature, string Condition, string IconUrl, double Humidity, double WindSpeed);

public class WeatherClientService : BaseClientService
{
    public WeatherClientService(HttpClient httpClient, IJSRuntime jsRuntime) : base(httpClient, jsRuntime) { }

    public async Task<Result<WeatherData>> GetWeatherAsync(string city)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.GetFromJsonAsync<Result<WeatherData>>($"/api/weather?city={city}");
            return response ?? Result<WeatherData>.CreateFailure("Failed to load weather data.");
        }
        catch (Exception ex)
        {
            return Result<WeatherData>.CreateFailure(ex.Message);
        }
    }
}
