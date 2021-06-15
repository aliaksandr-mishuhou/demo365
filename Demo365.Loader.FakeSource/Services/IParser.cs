using Demo365.Contracts;
using System.Collections.Generic;

namespace Demo365.Loader.FakeSource.Services
{
    public interface IParser
    {
        IAsyncEnumerable<IEnumerable<Game>> GetAsync();
    }
}
