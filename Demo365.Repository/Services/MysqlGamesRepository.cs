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
    /// <summary>
    /// simple MariaDB/MySQL repository
    /// check DB Schema in setup.sql
    /// 
    /// TODO: use stored procedures
    /// TODO: add paging
    /// TODO: add retries
    /// </summary>
    public class MysqlGamesRepository : IGamesRepository
    {
        private readonly IDbRouter _router;
        private readonly ILogger<MysqlGamesRepository> _logger;
        private readonly Config _config;

        // TODO: better to replace with SP
        private const string CheckSqlTemplate = @"
            SELECT COUNT(*) AS duplicates FROM games 
            WHERE sport = @Sport 
                AND competition = @Competition 
                AND (team1 = @Team1 AND team2 = @Team2 OR team1 = @Team2 AND team2 = @Team1) 
                AND time >= DATE_ADD(@Time, INTERVAL -{0} MINUTE) AND time < DATE_ADD(@Time, INTERVAL {0} MINUTE)";


        // TODO: better to replace with SP
        private const string InsertSqlTeample = @"
            INSERT INTO games(sport, competition, team1, team2, time)
            VALUES (@Sport, @Competition, @Team1, @Team2, @Time)";

        // TODO: better to replace with SP
        // TODO: add pagination, replace hardcoded limit = 100
        private const string SearchSqlTemplate = @"
            SELECT * FROM games 
            /**where**/ 
            ORDER BY time DESC 
            LIMIT 0, 100";

        public MysqlGamesRepository(IDbRouter router, ILogger<MysqlGamesRepository> logger, Config config = null)
        {
            _router = router;
            _logger = logger;

            _config = config;
            if (_config == null) 
            {
                _config = new Config();
            }
        }

        public async Task<int> AddAsync(IEnumerable<Game> games)
        {
            var result = 0;
            var batchId = DateTime.UtcNow.Ticks;

            try
            {
                var gameIndexes = games.Select(g => ToDbEntity(g)).ToArray();

                // NOTE: assume that we receive only batches with a single sport type
                var sport = gameIndexes.FirstOrDefault()?.Sport;

                if (sport == null) 
                {
                    return result;
                }

                // TODO: move check & insert operations to DB level (a single stored procedure)
                var connectionString = _router.GetConnectionString(new DbRouterSettings { Sport = sport });
                using (var conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // skip duplicates
                    var halfInterval = Convert.ToInt32( _config.UniqueIntervalMinutes * 1f / 2);
                    var checkSql = string.Format(CheckSqlTemplate, halfInterval);

                    var filtered = new List<GameIndex>();
                    foreach (var gameIndex in gameIndexes)
                    {
                        var check = await conn.QueryFirstOrDefaultAsync<Check>(checkSql, gameIndex);

                        if (check != null && check.Duplicates > 0)
                        {

                            _logger.LogDebug($"Ignoring duplicate [{gameIndex}]");
                            continue;
                        }

                        _logger.LogDebug($"Marked for adding [{gameIndex}]");
                        filtered.Add(gameIndex);
                    }

                    // insert batch
                    result = await conn.ExecuteAsync(InsertSqlTeample, filtered);
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
                var connectionString = _router.GetConnectionString(new DbRouterSettings { Sport = search.Sport });
                using (var conn = new MySqlConnection(connectionString))
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

            var selector = builder.AddTemplate(SearchSqlTemplate);

            // basic filtering (full match)

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

            // TODO: paging support

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

        public class Config 
        {
            private const int DefaultUniqueGameIntervalMinutes = 120;

            public int UniqueIntervalMinutes { get; set; } = DefaultUniqueGameIntervalMinutes;
        }
    }
}
