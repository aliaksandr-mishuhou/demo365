using Demo365.Contracts;
using System.Threading.Tasks;

namespace Demo365.Api.Services
{
    public interface ISearchService
    {
        public Task<SearchResult> SearchAsync(SearchRequest request);
    }
}
