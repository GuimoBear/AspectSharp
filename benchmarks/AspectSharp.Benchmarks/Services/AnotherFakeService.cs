using AspectSharp.Benchmarks.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks.Services
{
    public sealed class AnotherFakeService : IAnotherFakeService
    {
        private readonly ILogger<AnotherFakeService> _logger;

        public AnotherFakeService(ILogger<AnotherFakeService> logger)
            => _logger = logger;

        public Task<string> SayHello(string param)
        {
            _logger.LogInformation("Another SayHello called: {0}", param);
            return Task.FromResult(string.Format("Hello {0}", param));
        }
    }
}
