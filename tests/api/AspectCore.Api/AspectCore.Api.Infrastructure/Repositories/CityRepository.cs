using AspectCore.Api.Domain.Entities;
using AspectCore.Api.Domain.Repositories.Interfaces;
using AspectCore.Api.Utils;

namespace AspectCore.Api.Infrastructure.Repositories
{
    public sealed class CityRepository : ICityRepository
    {
        public async Task<City> Find(string cityName)
        {
            await Delay.Random();
            return new City(Guid.NewGuid(), cityName, Random.Shared.Next(1, 30));
        }
    }
}
