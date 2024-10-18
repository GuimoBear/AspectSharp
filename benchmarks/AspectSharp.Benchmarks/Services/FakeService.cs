using AspectSharp.Benchmarks.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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

        public string SayHelloWithFourInterceptors(string param)
        {
            _logger.LogInformation("SayHello called: {0}", param);
            return string.Format("Hello {0}", param);
        }

        public async Task<string> SayHelloAsync(string param)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(0.005));
            _logger.LogInformation("SayHello called: {0}", param);
            return string.Format("Hello {0}", param);
        }

        public async Task<string> SayHelloWithFourInterceptorsAsync(string param)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(0.005));
            _logger.LogInformation("SayHello called: {0}", param);
            return string.Format("Hello {0}", param);
        }

        public string SayHelloWithoutMetrics(string param)
        {
            _logger.LogInformation("SayHelloWithoutAspects called: {0}", param);
            return string.Format("Hello {0}", param);
        }

        public async Task<string> SayHelloWithoutMetricsAsync(string param)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(0.005));
            _logger.LogInformation("SayHelloWithoutAspects called: {0}", param);
            return string.Format("Hello {0}", param);
        }
    }
}
