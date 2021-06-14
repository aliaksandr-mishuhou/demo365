using Demo365.Contracts;
using Demo365.ServiceBus;
using Demo365.ServiceBus.Kafka;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Demo365.Repository.Services
{
    public class AddGamesConsumer : ConfluentKafkaConsumer<AddRequest>
    {
        private readonly IGamesRepository _gamesRepository;
        private readonly ILogger<AddGamesConsumer> _logger;

        public AddGamesConsumer(IGamesRepository gamesRepository, KafkaConsumerConfig config, ILogger<AddGamesConsumer> logger) 
            : base(config, logger)
        {
            _gamesRepository = gamesRepository;
            _logger = logger;
        }

        public override async Task HandleAsync(AddRequest message, CancellationToken cancellationToken)
        {
            var result = await _gamesRepository.AddAsync(message.Items);

            _logger.LogInformation($"Added {result} games (async)");

            await base.HandleAsync(message, cancellationToken);
        }
    }
}
