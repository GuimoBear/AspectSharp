using AspectCore.Api.Domain.Entities;
using AspectCore.Api.Domain.Responses;

namespace AspectCore.Api.Domain.Clients.Interfaces
{
    public interface IWeatherForecastClient
    {
        Task<WeatherForecast> Get(City city, DateTime date);
    }
}
