using Demo365.Contracts;
using System;
using System.Threading.Tasks;

namespace Demo365.Api.Services
{
    public class NullCacheRepository : IDistributedCacheRepository
    {
        public Task<SearchResult> GetAsync(SearchRequest request)
        {
            return Task.FromResult<SearchResult>(null);
        }

        public Task SetAsync(SearchRequest request, SearchResult result, TimeSpan ttl)
        {
            return Task.CompletedTask;
        }
    }
}
