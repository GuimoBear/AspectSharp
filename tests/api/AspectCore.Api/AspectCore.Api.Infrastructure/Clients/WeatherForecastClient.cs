using AspectCore.Api.Domain.Clients.Interfaces;
using AspectCore.Api.Domain.Entities;
using AspectCore.Api.Domain.Responses;
using AspectCore.Api.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectCore.Api.Infrastructure.Clients
{
    public sealed class WeatherForecastClient : IWeatherForecastClient
    {
        public async Task<WeatherForecast> Get(City city, DateTime date)
        {
            await Delay.Random();
            return new WeatherForecast
            {
                Date = date,
                TemperatureC = Random.Shared.Next(23, 32)
            };
        }
    }
}
