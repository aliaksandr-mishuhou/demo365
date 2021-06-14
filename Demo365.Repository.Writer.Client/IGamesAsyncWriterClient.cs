using Demo365.Contracts;
using System.Threading.Tasks;

namespace Demo365.Repository.Writer.Client
{
    public interface IGamesAsyncWriterClient
    {
        /// <summary>
        /// Async flow (via message queue)
        /// </summary>
        /// <param name="games"></param>
        /// <returns></returns>
        Task EnqueueAsync(AddRequest addRequest);
    }
}
