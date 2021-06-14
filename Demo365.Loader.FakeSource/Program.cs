using Demo365.Loader.FakeSource.Services;
using Demo365.Repository.Writer.Client;
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
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    services.AddSingleton<IParser, FakeParser>();
                    services.AddSingleton<IProcessor, Processor>();
                    services.AddSingleton<IGamesSyncWriterClient, RestGamesWriterClient>(
                        options => new RestGamesWriterClient(
                            hostContext.Configuration.GetValue<string>("REPOSITORY_URL"),
                            options.GetService<ILogger<RestGamesWriterClient>>()
                        ));
                });
    }
}
