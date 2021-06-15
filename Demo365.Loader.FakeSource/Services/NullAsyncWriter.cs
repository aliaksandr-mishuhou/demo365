using Demo365.Contracts;
using Demo365.Repository.Writer.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Demo365.Loader.FakeSource.Services
{
    public class NullAsyncWriter : IGamesAsyncWriterClient
    {
        private readonly ILogger<NullAsyncWriter> _logger;

        public NullAsyncWriter(ILogger<NullAsyncWriter> logger)
        {
            _logger = logger;
        }

        public Task EnqueueAsync(AddRequest addRequest)
        {
            _logger.LogWarning("Temporary disabled. Use 'sync' flow instead");
            return Task.CompletedTask;
        }
    }
}
