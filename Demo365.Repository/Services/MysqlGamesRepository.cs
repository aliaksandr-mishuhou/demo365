using Dapper;
using Demo365.Contracts;
using Demo365.Repository.Services.Dto;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo365.Repository.Services
{
    public class MysqlGamesRepository : IGamesRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MysqlGamesRepository> _logger;

        // private const int RetryCount = 1;

        private const string CheckSql = @"
SELECT COUNT(*) AS duplicates FROM games 
WHERE sport = @Sport 
    AND competition = @Competition 
    AND (team1 = @Team1 AND team2 = @Team2 OR team1 = @Team2 AND team2 = @Team1) 
    AND time >= DATE_ADD(@Time, INTERVAL -5 MINUTE) AND time < DATE_ADD(@Time, INTERVAL 5 MINUTE)";


        private const string InsertSql = @"
INSERT INTO games(sport, competition, team1, team2, time)
VALUES (@Sport, @Competition, @Team1, @Team2, @Time)";

        private const string SearchSql = "SELECT * FROM games /**where**/";

        public MysqlGamesRepository(string connectionString, ILogger<MysqlGamesRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<int> AddAsync(IEnumerable<Game> games)
        {
            var result = 0;
            var batchId = DateTime.UtcNow.Ticks;

            try
            {
                var gameIndexes = games.Select(g => ToDbEntity(g)).ToArray();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // skip duplicates
                    var filtered = new List<GameIndex>();
                    foreach (var gameIndex in gameIndexes)
                    {

                        var check = await conn.QueryFirstOrDefaultAsync<Check>(CheckSql, gameIndex);

                        if (check != null && check.Duplicates > 0) 
                        {

                            _logger.LogDebug($"Ignoring duplicate [{gameIndex}]");
                            continue;
                        }

                        _logger.LogDebug($"Marked for adding [{gameIndex}]");
                        filtered.Add(gameIndex);
                    }

                    // insert batch
                    result = await conn.ExecuteAsync(InsertSql, filtered);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not save data to DB for {batchId}", ex);
            }

            return result;
        }

        public async Task<IEnumerable<Game>> SearchAsync(SearchRequest search)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var selector = BuildSelector(search);

                    _logger.LogDebug($"Querying [{search}]");

                    var gameIndexes = await conn.QueryAsync<GameIndex>(selector.RawSql, selector.Parameters);

                    var result = gameIndexes.Select(gi => FromDbEntity(gi));
                    _logger.LogInformation($"Found {result.Count()} items for [{search}]");

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not execute search for [{search}]", ex);
                return Enumerable.Empty<Game>();
            }
        }

        private static SqlBuilder.Template BuildSelector(SearchRequest search)
        {
            var builder = new SqlBuilder();

            var selector = builder.AddTemplate(SearchSql);

            if (search.FromTime != null)
            {
                builder.Where("time >= @FromTime", new { search.FromTime });
            }

            if (search.ToTime != null)
            {
                builder.Where("time < @ToTime", new { search.ToTime });
            }

            if (search.Sport != null)
            {
                builder.Where("sport = @Sport", new { search.Sport });
            }

            if (search.Competition != null)
            {
                builder.Where("competition = @Competition", new { search.Competition });
            }

            if (search.Team != null)
            {
                builder.Where("(team1 = @Team OR team2 = @Team)", new { search.Team });
            }

            return selector;
        }

        private static GameIndex ToDbEntity(Game game) 
        {
            var teams = game.Teams
                //.OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToArray();

            return new GameIndex 
            {
                Sport = game.Sport,
                Competition = game.Competition,
                Team1 = teams[0],
                Team2 = teams[1],
                Time = game.Time
            };
        }

        private static Game FromDbEntity(GameIndex gameIndex) 
        {
            return new Game 
            {
                Sport = gameIndex.Sport,
                Competition = gameIndex.Competition,
                Teams = new[] 
                { 
                    new GameTeam { Name = gameIndex.Team1 }, 
                    new GameTeam { Name = gameIndex.Team2 } 
                },
                Time = gameIndex.Time
            };
        }
    }
}
