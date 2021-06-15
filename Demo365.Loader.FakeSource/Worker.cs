using System;
using System.Threading;
using System.Threading.Tasks;
using Demo365.Loader.FakeSource.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo365.Loader.FakeSource
{
    public class Worker : BackgroundService
    {
        private const int IntervalSeconds = 60;
        private readonly ILogger<Worker> _logger;
        private readonly IProcessor _processor;

        public Worker(ILogger<Worker> logger, IProcessor processor)
        {
            _logger = logger;
            _processor = processor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.UtcNow);
                try
                {
                    await _processor.RunAsync();
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, "Could not process");
                }

                await Task.Delay(1000 * IntervalSeconds, stoppingToken);
            }
        }
    }
}
