using Demo365.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo365.Loader.FakeSource.Services
{
    public interface IParser
    {
        Task<IEnumerable<Game>> GetAsync();
    }
}
