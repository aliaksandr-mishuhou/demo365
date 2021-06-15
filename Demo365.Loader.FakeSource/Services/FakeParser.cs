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

        public async IAsyncEnumerable<IEnumerable<Game>> GetAsync()
        {
            var competition1 = GenerateCompetition("football", "UK",
                new string[] { "Arcenal", "Man United", "Everton", "Chelsea", "Man City", "Newcastle" });
            yield return await competition1;

            var competition2 = GenerateCompetition("football", "EURO-2020",
                new string[] { "England", "Germany", "Spain", "France", "Italy", "Turkey", "Greece", "Portugal", "Sweden" });
            yield return await competition2;

            var competition3 = GenerateCompetition("tennis", "C3",
                new string[] { "T1", "T2", "T3", "T4" });
            yield return await competition3;

            var competition4 = GenerateCompetition("basketball", "C4",
                new string[] { "B1", "B2", "B3", "B4" });
            yield return await competition4;

            var competition5 = GenerateCompetition("swimming", "helloworld",
                new string[] 
                {
                    GenerateName(0, 5),
                    GenerateName(5, 10),
                    GenerateName(10, 15),
                    GenerateName(15, 20),
                    GenerateName(20, 25),
                    GenerateName(25, 30),
                });
            yield return await competition5;
        }

        private string GenerateName(int from, int to) 
        {
            return $"player{_random.Next(from, to)}";
        }

        private Task<IEnumerable<Game>> GenerateCompetition(string sport, string competition, string[] teams) 
        {
            var games = new List<Game>();
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
                    Time = DateTime.UtcNow,
                };

                games.Add(game);
            }

            return Task.FromResult(games.Cast<Game>());
        }
    }
}
