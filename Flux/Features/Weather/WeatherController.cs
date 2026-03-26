using Flux.Domain.Common;
using Flux.Infrastructure.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Weather
{
    [Authorize]
    [ApiController]
    [Route("api/weather")]
    public class WeatherController : ControllerBase
    {
        private readonly IConfiguration _config;

        public WeatherController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<Result<WeatherData>>> GetWeather(string city)
        {
            // For a real app, we'd use an API key like this:
            // var apiKey = _config["Weather:ApiKey"];
            // but for now we'll mock it since we don't have one
            
            await Task.Delay(500); // Simulate network latency

            var random = new Random();
            var data = new WeatherData(
                city,
                random.Next(15, 35),
                "Sunny",
                "https://openweathermap.org/img/wn/01d@2x.png",
                random.Next(40, 70),
                random.Next(5, 20)
            );

            return Ok(Result<WeatherData>.CreateSuccess(data));
        }
    }
}
