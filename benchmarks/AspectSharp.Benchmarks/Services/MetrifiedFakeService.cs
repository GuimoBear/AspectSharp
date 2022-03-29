using AspectSharp.Benchmarks.Clients.Interfaces;
using AspectSharp.Benchmarks.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks.Services
{
    public class MetrifiedFakeService : IMetrifiedFakeService
    {

        private readonly ILogger<MetrifiedFakeService> _logger;
        private readonly IMetricsClient _metrics;

        public MetrifiedFakeService(ILogger<MetrifiedFakeService> logger, IMetricsClient metrics)
        {
            _logger = logger;
            _metrics = metrics;
        }

        public string SayHello(string param)
        {
            //using (var typeScope = _metrics.NewScope(string.Format("Class {0}.{1}", typeof(MetrifiedFakeService).FullName, nameof(MetrifiedFakeService.SayHello))))
            using (var methodScope = _metrics.NewScope(string.Format("Method {0}.{1}", typeof(MetrifiedFakeService).FullName, nameof(MetrifiedFakeService.SayHello))))
            {
                _logger.LogInformation("SayHello called: {0}", param);
                return string.Format("Hello {0}", param);
            }
        }

        public async Task<string> SayHelloAsync(string param)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(0.005));
            //using (var typeScope = _metrics.NewScope(string.Format("Class {0}.{1}", typeof(MetrifiedFakeService).FullName, nameof(MetrifiedFakeService.SayHello))))
            using (var methodScope = _metrics.NewScope(string.Format("Method {0}.{1}", typeof(MetrifiedFakeService).FullName, nameof(MetrifiedFakeService.SayHello))))
            {
                _logger.LogInformation("SayHello called: {0}", param);
                return string.Format("Hello {0}", param);
            }
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
