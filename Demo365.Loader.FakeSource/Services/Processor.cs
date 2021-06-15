using Demo365.Contracts;
using Demo365.Repository.Writer.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo365.Loader.FakeSource.Services
{
    public class Processor : IProcessor
    {
        private readonly ILogger<Processor> _logger;
        private readonly IParser _parser;
        private readonly IGamesSyncWriterClient _gamesSyncWriterClient;
        private readonly IGamesAsyncWriterClient _gamesAsyncWriterClient;
        private readonly bool _asyncMode;

        public Processor(
            IParser parser, 
            IGamesSyncWriterClient gamesSyncWriterClient, 
            IGamesAsyncWriterClient gamesAsyncWriterClient,
            ProcessorArgs args,
            ILogger<Processor> logger)
        {
            _logger = logger;
            _parser = parser;
            _gamesSyncWriterClient = gamesSyncWriterClient;
            _gamesAsyncWriterClient = gamesAsyncWriterClient;
            _asyncMode = args.Async;
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Parsing...");
            var batch = 0;
            await foreach (var items in _parser.GetAsync()) 
            {
                _logger.LogInformation($"Sending batch = {batch} ...");
                await ProcessBatchAsync(items);
                _logger.LogInformation($"Sent batch = {batch}, items = {items.Count()}");
                batch++;
            }
        }

        private async Task ProcessBatchAsync(IEnumerable<Game> items) 
        {
            var addRequest = new AddRequest { Items = items };

            var addRequestJson = JsonConvert.SerializeObject(addRequest);
            _logger.LogDebug(addRequestJson);

            if (_asyncMode)
            {
                _logger.LogDebug("Sending data (async)...");
                await _gamesAsyncWriterClient.EnqueueAsync(addRequest);
                _logger.LogInformation("Sent data (async)");
            }
            else
            {
                _logger.LogDebug("Sending data (sync)...");
                var addResult = await _gamesSyncWriterClient.WriteAsync(addRequest);
                _logger.LogInformation($"Sent data (sync), added = {addResult.Added}");
            }

        }
    }
}
