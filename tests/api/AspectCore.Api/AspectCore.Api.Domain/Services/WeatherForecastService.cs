using AspectCore.Api.Domain.Clients.Interfaces;
using AspectCore.Api.Domain.Repositories.Interfaces;
using AspectCore.Api.Domain.Requests;
using AspectCore.Api.Domain.Responses;
using AspectCore.Api.Domain.Services.Interfaces;
using AspectCore.Api.Utils;

namespace AspectCore.Api.Domain.Services
{
    public sealed class WeatherForecastService : IWeatherForecastService
    {
        private readonly ICityRepository _cityRepository;
        private readonly IWeatherForecastClient _forecastClient;

        public WeatherForecastService(ICityRepository cityRepository, IWeatherForecastClient forecastClient)
        {
            _cityRepository = cityRepository;
            _forecastClient = forecastClient;
        }

        public async Task<WeatherForecast> Get(WeatherForecastRequest request)
        {
            var city = await _cityRepository.Find(request.City);
            await Delay.Random();
            return await _forecastClient.Get(city, request.Date);
        }
    }
}
