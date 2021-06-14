using Demo365.Contracts;
using Demo365.ServiceBus;
using System.Threading.Tasks;

namespace Demo365.Repository.Writer.Client
{
    public class QueueGamesWriterClient : IGamesAsyncWriterClient
    {
        private readonly IPublisher _publisher;

        public QueueGamesWriterClient(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task EnqueueAsync(AddRequest addRequest)
        {
            await _publisher.SendAsync(addRequest);
        }
    }
}
