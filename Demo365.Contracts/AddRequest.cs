using System.Collections.Generic;

namespace Demo365.Contracts
{
    public class AddRequest
    {
        public IEnumerable<Game> Items { get; set; }
        public string Source { get; set; }
    }
}
