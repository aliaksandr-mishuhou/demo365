using Demo365.Contracts;
using System.Threading.Tasks;

namespace Demo365.Repository.Writer.Client
{
    public interface IGamesSyncWriterClient
    {
        /// <summary>
        /// Sync flow (via HTTP REST)
        /// </summary>
        /// <param name="games"></param>
        /// <returns></returns>
        Task<AddResult> WriteAsync(AddRequest addRequest);
    }
}
