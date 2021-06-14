using Demo365.Contracts;
using System;
using System.Threading.Tasks;

namespace Demo365.Api.Services
{
    public interface ICacheRepository
    {
        Task<SearchResult> GetAsync(SearchRequest request);
        Task SetAsync(SearchRequest request, SearchResult result, TimeSpan ttl);
    }
}
