using Demo365.Contracts;
using Demo365.Repository.Query.Client;
using System;
using System.Threading.Tasks;

namespace Demo365.Api.Services
{
    public class SearchService : ISearchService
    {
        private const int ProcessTtlSeconds = 60;
        private const int DistributedTtlSeconds = 60 * 5;

        private readonly IGamesQueryClient _gamesQueryClient;
        private readonly IProcessCacheRepository _processCacheRepository;
        private readonly IDistributedCacheRepository _distributedCacheRepository;

        public SearchService(IGamesQueryClient gamesQueryClient, 
            IProcessCacheRepository processCacheRepository, 
            IDistributedCacheRepository distributedCacheRepository)
        {
            _gamesQueryClient = gamesQueryClient;
            _processCacheRepository = processCacheRepository;
            _distributedCacheRepository = distributedCacheRepository;
        }

        public async Task<SearchResult> SearchAsync(SearchRequest request)
        {
            var result = await _processCacheRepository.GetAsync(request);

            if (result == null) 
            {
                result = await _distributedCacheRepository.GetAsync(request);

                if (result == null) 
                {
                    result = await _gamesQueryClient.SearchAsync(request);
                    await _distributedCacheRepository.SetAsync(request, result, TimeSpan.FromSeconds(DistributedTtlSeconds));
                }

                await _processCacheRepository.SetAsync(request, result, TimeSpan.FromSeconds(ProcessTtlSeconds));
            }

            return result;
        }
    }
}
