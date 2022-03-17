using AspectSharp.Benchmarks.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AspectSharp.Benchmarks.Services
{
    public class FakeService : IFakeService
    {
        private readonly ILogger<FakeService> _logger;

        public FakeService(ILogger<FakeService> logger)
            => _logger = logger;

        public string SayHello(string param)
        {
            _logger.LogInformation("SayHello called: {0}", param);
            return string.Format("Hello {0}", param);
        }

        public string SayHelloWithoutMetrics(string param)
        {
            _logger.LogInformation("SayHelloWithoutAspects called: {0}", param);
            return string.Format("Hello {0}", param);
        }
    }
}
