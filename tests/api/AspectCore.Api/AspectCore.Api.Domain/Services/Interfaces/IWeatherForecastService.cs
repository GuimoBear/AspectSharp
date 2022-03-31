using AspectCore.Api.Domain.Requests;
using AspectCore.Api.Domain.Responses;

namespace AspectCore.Api.Domain.Services.Interfaces
{
    public interface IWeatherForecastService
    {
        Task<WeatherForecast> Get(WeatherForecastRequest request);
    }
}
