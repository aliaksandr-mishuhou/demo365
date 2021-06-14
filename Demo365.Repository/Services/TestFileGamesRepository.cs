using Demo365.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Demo365.Repository.Services
{
    /// <summary>
    /// IGNORE / TESTING PURPOSES only
    /// this is not a real DB shared accross several instance
    /// just for testing purposes
    /// </summary>
    public class TestFileGamesRepository : IGamesRepository
    {
        private const string DefaultPath = "demo.games.db";

        private List<Game> _games = new List<Game>();
        private readonly string _path;

        public TestFileGamesRepository(string path = DefaultPath)
        {
            _path = path;
            RestoreAsync().Wait();
        }

        public async Task<int> AddAsync(IEnumerable<Game> games)
        {
            foreach (var game in games) 
            {
                if (string.IsNullOrEmpty(game.Id)) 
                {
                    game.Id = Guid.NewGuid().ToString();
                }
            }

            _games.AddRange(games);
            await BackupAsync();
            return games.Count();
        }

        public Task<IEnumerable<Game>> SearchAsync(SearchRequest search)
        {
            return Task.FromResult(_games.Cast<Game>());
        }

        private async Task BackupAsync() 
        {
            var content = JsonConvert.SerializeObject(_games);
            await File.WriteAllTextAsync(_path, content);
        }

        private async Task RestoreAsync()
        {
            if (!File.Exists(_path))
            {
                return;
            }

            var content = await File.ReadAllTextAsync(_path);
            _games = JsonConvert.DeserializeObject<List<Game>>(content);
        }
    }
}
