﻿using Demo365.Contracts;
using Demo365.Repository.Writer.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        public Processor(ILogger<Processor> logger,
            IParser parser, 
            IGamesSyncWriterClient gamesSyncWriterClient, 
            IGamesAsyncWriterClient gamesAsyncWriterClient = null,
            bool asyncMode = false)
        {
            _logger = logger;
            _parser = parser;
            _gamesSyncWriterClient = gamesSyncWriterClient;
            _gamesAsyncWriterClient = gamesAsyncWriterClient;
            _asyncMode = asyncMode;
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Parsing...");
            var items = await _parser.GetAsync();
            _logger.LogInformation($"Parsed {items.Count()}");

            var addRequest = new AddRequest { Items = items };

            var addRequestJson = JsonConvert.SerializeObject(addRequest);
            _logger.LogDebug(addRequestJson);

            if (_asyncMode)
            {
                _logger.LogInformation("Sending data (async)...");
                await _gamesAsyncWriterClient.EnqueueAsync(addRequest);
                _logger.LogInformation("Sent data (async)");
            }
            else 
            {
                _logger.LogInformation("Sending data (sync)...");
                var addResult = await _gamesSyncWriterClient.WriteAsync(addRequest);
                _logger.LogInformation($"Sent data (sync), added = {addResult.Added}");
            }
        }
    }
}