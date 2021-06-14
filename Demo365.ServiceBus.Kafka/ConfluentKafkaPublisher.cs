using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Demo365.ServiceBus.Kafka
{
    public class ConfluentKafkaPublisher : IPublisher
    {
        private const int ConnectionTimeoutMilliseconds = 3000;
        private const int StatisticsIntervalMilliseconds = 30000;

        private readonly IProducer<Null, string> _producer;
        private readonly IProducer<string, string> _producerWithKey;
        private readonly IStringSerializer _stringSerializer;
        private readonly KafkaProducerConfig _config;
        private readonly ILogger<ConfluentKafkaPublisher> _logger;

        internal ConfluentKafkaPublisher(
            KafkaProducerConfig config,
            ILogger<ConfluentKafkaPublisher> logger,
            IProducer<Null, string> producer,
            IProducer<string, string> producerWithKey,
            IStringSerializer stringSerializer = null)
        {
            _config = config;
            _logger = logger;
            _stringSerializer = stringSerializer ?? new JsonStringSerializer();
            _producer = producer;
            _producerWithKey = producerWithKey;
        }

        public ConfluentKafkaPublisher(KafkaProducerConfig config, ILogger<ConfluentKafkaPublisher> logger,
            IStringSerializer stringSerializer = null/*, KafkaBatchProducerConfig batchConfig = null*/)
        {
            _config = config;
            _logger = logger;
            _stringSerializer = stringSerializer ?? new JsonStringSerializer();

            var settings = new Dictionary<string, string>
            {
                {"bootstrap.servers", _config.ConnectionString},
                {"client.id", Dns.GetHostName()},
                {"request.timeout.ms", ConnectionTimeoutMilliseconds.ToString()},
                {"socket.timeout.ms", (ConnectionTimeoutMilliseconds * 2).ToString()},
                {"statistics.interval.ms", StatisticsIntervalMilliseconds.ToString()},
                {"retries", "0"},
                {"acks", "0"}
            };

            //if (batchConfig != null)
            //{
            //    settings.Add("batch.num.messages", (batchConfig?.MessageCount ?? 1).ToString());
            //    settings.Add("socket.blocking.max.ms", (batchConfig?.SocketBlockingIntervalMs ?? 1).ToString());
            //    settings.Add("socket.nagle.disable", (!batchConfig?.Nagle ?? false).ToString()?.ToLower());
            //    settings.Add("queue.buffering.max.ms", (batchConfig?.IntervalMs ?? 1).ToString());
            //}

            //var eventHandler = new KafkaLifeCycleEventHandler(_logger, config.LoggerSettings);

            _producer = new ProducerBuilder<Null, string>(settings)
                //.SetErrorHandler(eventHandler.HandleError)
                //.SetLogHandler(eventHandler.HandleLog)
                //.SetStatisticsHandler(eventHandler.HandleStatistic)
                .Build();

            _producerWithKey = new ProducerBuilder<string, string>(settings)
                //.SetErrorHandler(eventHandler.HandleError)
                //.SetLogHandler(eventHandler.HandleLog)
                //.SetStatisticsHandler(eventHandler.HandleStatistic)
                .Build();

            _logger.LogInformation(
                $"Created producer, config = [{config}]; Settings: {_stringSerializer.Serialize(settings)}");
        }

        public bool Send<T>(T message, string groupKey = null)
            => SendAsync(message, groupKey).Result;

        public Task<bool> SendAsync<T>(T message, string groupKey = null)
            => SendRawMultipleTopicsAsync(_stringSerializer.Serialize(message), _config.TopicResolver.Resolve(message), groupKey);

        public Task<bool> SendRawAsync<T>(T message, string topic = null, string groupKey = null)
            => SendRawAsync(_stringSerializer.Serialize(message), topic, groupKey);

        public Task SendBatchAsync<T>(IEnumerable<T> messages)
            => SendBatchInternal(messages, message => SendAsync(message));

        public Task SendBatchRawAsync(IEnumerable<string> messages)
            => SendBatchInternal(messages, message => SendRawMultipleTopicsAsync(message, _config.TopicResolver.Resolve(message)));

        public async Task<bool> SendRawAsync(string json, string topic = null, string groupKey = null)
        {
            var topicsToSendTo = string.IsNullOrEmpty(topic)
                ? _config.TopicResolver?.Resolve(json)
                : new[] { topic };

            return await SendRawMultipleTopicsAsync(json, topicsToSendTo, groupKey);
        }

        public void Dispose()
        {
            _producer?.Dispose();
            _producerWithKey?.Dispose();
        }

        private async Task<bool> SendRawMultipleTopicsAsync(string json, IEnumerable<string> topicsToSendTo, string groupKey = null)
        {
            var topicsToSendToList = topicsToSendTo
                ?.Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            if (topicsToSendToList == null || !topicsToSendToList.Any())
            {
                _logger.LogError(
                    $"Unable to resolve topic for json: {json}; " +
                    $"groupKey: {groupKey}; " +
                    $"topicResolver: { _config.TopicResolver}");

                return false;
            }

            var results = await Task.WhenAll(topicsToSendToList.Select(t => SendInternal(json, t, groupKey)));
            return results.All(r => r);
        }

        private async Task<bool> SendInternal(string json, string topicToSend, string groupKey = null)
            => groupKey != null
                ? await SendInternal(_producerWithKey, topicToSend, new Message<string, string> { Value = json, Key = groupKey })
                : await SendInternal(_producer, topicToSend, new Message<Null, string> { Value = json });

        private async Task<bool> SendInternal<TKey, TMessage>(IProducer<TKey, TMessage> producer, string topic,
            Message<TKey, TMessage> message)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                var result = await producer.ProduceAsync(topic, message);
                _logger.LogDebug(
                    $"Send to {topic} {JsonHelper.EscapeJson(message.Value.ToString())}. Result: elapsedMs={timer.ElapsedMilliseconds} offset={result.Offset}, error={result.Status}");
                return result.Status == PersistenceStatus.Persisted;

            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(
                    $"Error while sending to {topic} {JsonHelper.EscapeJson(message.Value.ToString())}, {_stringSerializer.Serialize(ex.DeliveryResult)}, {_stringSerializer.Serialize(ex.Error)}",
                    ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while sending to {topic} {JsonHelper.EscapeJson(message.Value.ToString())}", ex);
            }
            finally
            {
                timer.Stop();
            }

            return false;
        }

        private async Task SendBatchInternal<T>(IEnumerable<T> messages, Func<T, Task<bool>> send)
        {
            if (messages?.Any() == true)
            {
                var producerData = messages.Select(async message => { await send(message); });
                await Task.WhenAll(producerData);
            }
        }
    }
}
