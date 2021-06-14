using System;

namespace Demo365.Repository.Services.Dto
{
    public class GameIndex
    {
        public long Id { get; set; }
        public string Sport { get; set; }
        public string Competition { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"sport = {Sport}, comp = {Competition}, t1 = {Team1}, t2 = {Team2}, time = {Time}";
        }
    }
}
