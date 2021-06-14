using Demo365.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo365.Loader.FakeSource.Services
{
    public class FakeParser : IParser
    {
        private Random _random = new Random(DateTime.UtcNow.Millisecond);

        public Task<IEnumerable<Game>> GetAsync()
        {
            var competition1 = GenerateCompetition("football", "UK",
                new string[] { "Arcenal", "Man United", "Everton", "Chelsea", "Man City", "Newcastle" });
            var competition2 = GenerateCompetition("football", "EURO-2020",
                new string[] { "England", "Germany", "Spain", "France", "Italy", "Turkey", "Greece", "Portugal", "Sweden" });
            var competition3 = GenerateCompetition("tennis", "C3",
                new string[] { "T1", "T2", "T3", "T4" });
            var competition4 = GenerateCompetition("basketball", "C4",
                new string[] { "B1", "B2", "B3", "B4" });

            var result = competition1.Union(competition2).Union(competition3).Union(competition4);

            return Task.FromResult(result);
        }

        private IEnumerable<Game> GenerateCompetition(string sport, string competition, string[] teams) 
        {
            var total = Convert.ToInt32(Math.Floor(teams.Length * 1f / 2));
            for (var i = 0; i < total; i++)
            {
                var gameTeam1 = new GameTeam 
                {
                    Name = teams[i],
                    Score = DateTime.UtcNow.Hour - _random.Next(0, DateTime.UtcNow.Hour)
                };

                var gameTeam2 = new GameTeam
                {
                    Name = teams[teams.Length - 1 - i],
                    Score = DateTime.UtcNow.Hour + _random.Next(0, 24 - DateTime.UtcNow.Hour)
                };

                var game = new Game 
                {
                    Sport = sport,
                    Competition = competition,
                    Teams = new[] { gameTeam1, gameTeam2 },
                    Time = DateTime.UtcNow
                };

                yield return game;
            }
        }
    }
}
