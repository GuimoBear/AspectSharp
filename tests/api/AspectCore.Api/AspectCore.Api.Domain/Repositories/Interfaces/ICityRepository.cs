using AspectCore.Api.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectCore.Api.Domain.Repositories.Interfaces
{
    public interface ICityRepository
    {
        Task<City> Find(string cityName);
    }
}
