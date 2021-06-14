using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Demo365.ServiceBus.Kafka
{
    public abstract class ConfluentKafkaConsumer<T> : IConsumer<T>
    {
        private const int ConnectionTimeoutMilliseconds = 3000;
        private const int StatisticsIntervalMilliseconds = 10000;

        private readonly IConsumer<Null, string> _consumer;
        private readonly IConsumer<string, string> _consumerKeyed;
        private readonly ILogger<ConfluentKafkaConsumer<T>> _logger;
        private readonly IStringSerializer _stringSerializer;
        private readonly bool _groupKey;
        private readonly string _topic;
        private readonly Task _task;

        protected readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

        protected ConfluentKafkaConsumer(KafkaConsumerConfig config, ILogger<ConfluentKafkaConsumer<T>> logger, IStringSerializer stringSerializer = null, bool groupKey = false)
        {
            _logger = logger;
            _stringSerializer = stringSerializer ?? new JsonStringSerializer();

            _groupKey = groupKey;

            var settings = new Dictionary<string, string>
            {
                {"group.id", config.GroupId},
                {"client.id", Dns.GetHostName()},
                {"request.timeout.ms", ConnectionTimeoutMilliseconds.ToString()},
                {"socket.timeout.ms", (ConnectionTimeoutMilliseconds * 2).ToString()},
                {"statistics.interval.ms", StatisticsIntervalMilliseconds.ToString()},
                {"socket.nagle.disable", "true"},
                {"auto.commit.interval.ms", "2000"},
                {"bootstrap.servers", config.ConnectionString},
                {"auto.offset.reset", config.AutoOffsetReset ?? "largest"}
            };
            _topic = config.Topic;
            //var eventHandler = new KafkaLifeCycleEventHandler(_logger, config.LoggerSettings);

            if (groupKey)
            {
                _consumerKeyed = new ConsumerBuilder<string, string>(settings)
                    //.SetErrorHandler(eventHandler.HandleError)
                    //.SetLogHandler(eventHandler.HandleLog)
                    //.SetStatisticsHandler(eventHandler.HandleStatistic)
                    //.SetOffsetsCommittedHandler(eventHandler.HandleOnOffsetCommit)
                    //.SetPartitionsAssignedHandler((x, y) => eventHandler.HandlePartitionsAssigned(x, y))
                    //.SetPartitionsRevokedHandler((x, y) => eventHandler.HandlePartitionsRevoked(x, y))
                    .Build();
            }
            else
            {
                _consumer = new ConsumerBuilder<Null, string>(settings)
                    //.SetErrorHandler(eventHandler.HandleError)
                    //.SetLogHandler(eventHandler.HandleLog)
                    //.SetStatisticsHandler(eventHandler.HandleStatistic)
                    //.SetOffsetsCommittedHandler(eventHandler.HandleOnOffsetCommit)
                    //.SetPartitionsAssignedHandler((x, y) => eventHandler.HandlePartitionsAssigned(x, y))
                    //.SetPartitionsRevokedHandler((x, y) => eventHandler.HandlePartitionsRevoked(x, y))
                    .Build();
            }

            var cancellationToken = CancellationSource.Token;
            _task = Task.Factory.StartNew(Run, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public virtual void Handle(T message)
        {
        }

        public virtual Task HandleAsync(T message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            CancellationSource.Cancel();
            _logger.LogInformation($"Status of task:{_task?.Status.ToString()}");
            try
            {
                _task?.Wait();
            }
            catch (AggregateException)
            {
                _logger.LogInformation($"Task canceled");
            }
            _consumer?.Close();
            _consumerKeyed?.Close();

            _logger.LogInformation("Consumer disposed");
            CancellationSource.Dispose();

            _logger.LogInformation($"CancelTokenSource disposed, Status of task:{_task?.Status}");
        }

        private void Run()
        {
            _logger.LogInformation($"Starting consumer on {_topic}");

            try
            {
                // subscribe and run loop
                if (_groupKey)
                {
                    StartConsume(_consumerKeyed);
                }
                else
                {
                    StartConsume(_consumer);
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
                _logger.LogInformation("Operation cancel exception");
            }
            catch (ConsumeException ex)
            {
                _logger.LogError($"Consumer failed with ConsumeException", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("Consumer failed on consume", ex);
            }
            finally
            {
                _logger.LogInformation($"Stopping consumer on {_topic}");
            }
        }

        private void StartConsume<TKey>(IConsumer<TKey, string> consumer)
        {
            consumer.Subscribe(_topic);
            while (!CancellationSource.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(CancellationSource.Token);
                if (consumeResult.IsPartitionEOF)
                {
                    continue;
                }
                OnMessage(consumeResult);
            }
        }

        private void OnMessage<TKey>(ConsumeResult<TKey, string> result)
        {
            if (CancellationSource.IsCancellationRequested)
            {
                return;
            }
            _logger.LogDebug(
                $"Received from {result.Topic}, key={result.Message.Key}, partition={result.Partition}, offset={result.Offset}, message=[{JsonHelper.EscapeJson(result.Message.Value)}]");

            try
            {
                var message = _stringSerializer.Deserialize<T>(result.Message.Value);
                Handle(message);
                HandleAsync(message, CancellationSource.Token).Wait(CancellationSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Error on message handling from {result.Topic}, partition={result.Partition}, key: {result.Message.Key}, offset={result.Offset}, message=[{JsonHelper.EscapeJson(result.Message.Value)}]", ex);
            }
        }
    }
}
