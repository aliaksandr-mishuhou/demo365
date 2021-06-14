using Demo365.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo365.Repository.Services
{
    public interface IGamesRepository
    {
        Task<int> AddAsync(IEnumerable<Game> games);
        Task<IEnumerable<Game>> SearchAsync(SearchRequest search);
    }
}
