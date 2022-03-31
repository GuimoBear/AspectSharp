using AspectCore.Api.Domain.Requests;
using AspectCore.Api.Domain.Responses;
using AspectCore.Api.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AspectCore.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherForecastService _forecastService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastService forecastService)
        {
            _logger = logger;
            _forecastService = forecastService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var ret = new List<WeatherForecast>();
            foreach (var city in Enumerable.Range(1, 5).Select(_ => Summaries[Random.Shared.Next(Summaries.Length)]))
                ret.Add(await _forecastService.Get(new WeatherForecastRequest(city, DateTime.Today.AddDays(1))));
            return ret;
        }
    }
}