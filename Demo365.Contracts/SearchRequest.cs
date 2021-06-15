using System;

namespace Demo365.Contracts
{
    public class SearchRequest
    {
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public string Sport { get; set; }
        public string Competition { get; set; }
        public string Team { get; set; }

        public int Offset { get; set; }
        public int PageSize { get; set; }
    }
}
