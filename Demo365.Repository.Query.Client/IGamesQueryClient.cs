using Demo365.Contracts;
using System.Threading.Tasks;

namespace Demo365.Repository.Query.Client
{
    public interface IGamesQueryClient
    {
        /// <summary>
        /// search via REST 
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        Task<SearchResult> SearchAsync(SearchRequest search);
    }
}
