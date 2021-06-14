using Demo365.Loader.FakeSource.Services;
using Demo365.Repository.Writer.Client;
using Demo365.ServiceBus;
using Demo365.ServiceBus.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo365.Loader.FakeSource
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((ctx, services) =>
                {
                    services.AddHostedService<Worker>();

                    services.AddSingleton<IParser, FakeParser>();

                    // sync flow
                    services.AddSingleton<IGamesSyncWriterClient, RestGamesWriterClient>(
                        options => new RestGamesWriterClient(
                            ctx.Configuration.GetValue<string>("REPOSITORY_URL"),
                            options.GetService<ILogger<RestGamesWriterClient>>()
                        ));

                    // async flow
                    services.AddSingleton(options => new KafkaProducerConfig 
                    { 
                        ConnectionString = ctx.Configuration.GetValue<string>("KAFKA_CONNECTION"), 
                        TopicResolver = new DefaultTopicResolver(ctx.Configuration.GetValue<string>("KAFKA_TOPIC")) 
                    });
                    services.AddSingleton<IPublisher, ConfluentKafkaPublisher>();
                    services.AddSingleton<IGamesAsyncWriterClient, QueueGamesWriterClient>();

                    services.AddSingleton(options => new ProcessorArgs 
                    { 
                        Async = ctx.Configuration.GetValue<bool?>("ASYNC").GetValueOrDefault() 
                    });
                    services.AddSingleton<IProcessor, Processor>();
                });
    }
}
