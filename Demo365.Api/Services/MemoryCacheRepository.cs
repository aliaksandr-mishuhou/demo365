using Demo365.Contracts;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Demo365.Api.Services
{
    public class MemoryCacheRepository : IProcessCacheRepository
    {
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public Task<SearchResult> GetAsync(SearchRequest request)
        {
            var item = _cache.Get<SearchResult>(GetKey(request));
            return Task.FromResult(item);
        }

        public Task SetAsync(SearchRequest request, SearchResult result, TimeSpan ttl)
        {
            _cache.Set(GetKey(request), result, ttl);
            return Task.CompletedTask;
        }

        private static string GetKey(SearchRequest request) 
        {
            return $"f:{request.FromTime}_t:{request.ToTime}_";
        }
    }
}
