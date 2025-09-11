using VictoryFC.Models;
using VictoryFC.Services;

namespace VictoryFC.Services
{
    public class GameDataService : IGameDataService
    {
        private readonly List<GameResult> _regularSeasonGames;
        private readonly List<GameResult> _spenceCupGames;
        private readonly string _teamName = "Victory FC";

        public GameDataService()
        {
            _regularSeasonGames = LoadRegularSeasonData();
            _spenceCupGames = LoadSpenceCupData();
        }

        public Task<List<Standing>> GetCurrentStandingsAsync(string division = "regular")
        {
            var games = division.ToLower() == "spence" ? _spenceCupGames : _regularSeasonGames;
            var teams = games.SelectMany(g => new[] { g.HomeTeam, g.AwayTeam }).Distinct().ToList();
            var standings = new List<Standing>();

            foreach (var team in teams)
            {
                var teamGames = games.Where(g => (g.HomeTeam == team || g.AwayTeam == team) && g.IsCompleted).ToList();
                var stats = CalculateTeamStats(team, teamGames);
                standings.Add(stats);
            }

            var result = standings.OrderByDescending(s => s.Pts).ThenByDescending(s => s.GD).ThenByDescending(s => s.GF).ToList();

            for (int i = 0; i < result.Count; i++)
            {
                result[i].Position = i + 1;
            }

            return Task.FromResult(result);
        }

        public Task<GameResult> GetNextGameAsync()
        {
            var result = _regularSeasonGames.Where(g => !g.IsCompleted && (g.HomeTeam == _teamName || g.AwayTeam == _teamName)).OrderBy(g => g.GameDate).FirstOrDefault();
            return Task.FromResult(result);
        }

        public Task<List<GameResult>> GetRecentResultsAsync(int count = 5)
        {
            var result = _regularSeasonGames.Where(g => g.IsCompleted && (g.HomeTeam == _teamName || g.AwayTeam == _teamName)).OrderByDescending(g => g.GameDate).Take(count).ToList();
            return Task.FromResult(result);
        }

        public Task<List<GameResult>> GetUpcomingGamesAsync(int count = 3)
        {
            var result = _regularSeasonGames.Where(g => !g.IsCompleted && (g.HomeTeam == _teamName || g.AwayTeam == _teamName)).OrderBy(g => g.GameDate).Take(count).ToList();
            return Task.FromResult(result);
        }

        public Task<List<GameResult>> GetAllMatchesAsync()
        {
            var allGames = _regularSeasonGames.Concat(_spenceCupGames).ToList();
            return Task.FromResult(allGames);
        }

        public Task<List<GameResult>> GetMatchesByCompetitionAsync(string competition = "all")
        {
            var matches = competition.ToLower() switch
            {
                "regular" => _regularSeasonGames,
                "spence" => _spenceCupGames,
                _ => _regularSeasonGames.Concat(_spenceCupGames).ToList()
            };
            return Task.FromResult(matches.ToList());
        }

        public Task<SeasonStats> GetSeasonStatsAsync()
        {
            var completedGames = _regularSeasonGames.Where(g => g.IsCompleted && (g.HomeTeam == _teamName || g.AwayTeam == _teamName)).ToList();
            var wins = completedGames.Count(g => IsVictoryWin(g));
            var draws = completedGames.Count(g => IsVictoryDraw(g));
            var losses = completedGames.Count(g => IsVictoryLoss(g));

            var stats = new SeasonStats
            {
                GamesPlayed = completedGames.Count,
                Wins = wins,
                Draws = draws,
                Losses = losses,
                GoalsFor = completedGames.Sum(g => g.HomeTeam == _teamName ? (g.HomeScore ?? 0) : (g.AwayScore ?? 0)),
                GoalsAgainst = completedGames.Sum(g => g.HomeTeam == _teamName ? (g.AwayScore ?? 0) : (g.HomeScore ?? 0)),
                Points = wins * 3 + draws,
                WinPercentage = completedGames.Count > 0 ? (double)wins / completedGames.Count * 100 : 0
            };

            return Task.FromResult(stats);
        }

        public Task<string> GetNextGameLocationAsync()
        {
            var nextGame = _regularSeasonGames.Where(g => !g.IsCompleted && (g.HomeTeam == _teamName || g.AwayTeam == _teamName)).OrderBy(g => g.GameDate).FirstOrDefault();
            return Task.FromResult(GetLocationAddress(nextGame?.Field));
        }

        private string GetLocationAddress(string field)
        {
            return field switch
            {
                "Heritage Green 5" => "355 First Rd W, Stoney Creek, ON L8E 0G5",
                "Mohawk 2" or "Mohawk 5" or "Mohawk 6" => "135 Fennell Ave W, Hamilton, ON L9C 1E9",
                "Hamilton Italian Centre" => "1145 Stone Church Rd E, Hamilton, ON L8W 3J6",
                "Shady Acres 1" => "1500 Shaver Rd, Ancaster, ON L9G 3K9",
                "Ancaster 3-A" => "363 Wilson St E, Ancaster, ON L9G 2B8",
                "Proto Field" => "65 Herkimer St, Hamilton, ON L8P 2G5",
                "Sackville" => "316 Sackville Hill Lane, Lower Sackville, NS B4C 2R9",
                _ => "355 First Rd W, Stoney Creek, ON L8E 0G5"
            };
        }

        private Standing CalculateTeamStats(string team, List<GameResult> games)
        {
            var wins = games.Count(g => (g.HomeTeam == team && (g.HomeScore ?? 0) > (g.AwayScore ?? 0)) || (g.AwayTeam == team && (g.AwayScore ?? 0) > (g.HomeScore ?? 0)));
            var draws = games.Count(g => (g.HomeScore ?? 0) == (g.AwayScore ?? 0));
            var losses = games.Count - wins - draws;
            var goalsFor = games.Sum(g => g.HomeTeam == team ? (g.HomeScore ?? 0) : (g.AwayScore ?? 0));
            var goalsAgainst = games.Sum(g => g.HomeTeam == team ? (g.AwayScore ?? 0) : (g.HomeScore ?? 0));

            var lastFive = games.OrderByDescending(g => g.GameDate).Take(5).Select(g =>
            {
                var isWin = (g.HomeTeam == team && (g.HomeScore ?? 0) > (g.AwayScore ?? 0)) || (g.AwayTeam == team && (g.AwayScore ?? 0) > (g.HomeScore ?? 0));
                var isDraw = (g.HomeScore ?? 0) == (g.AwayScore ?? 0);
                var opponent = g.HomeTeam == team ? g.AwayTeam : g.HomeTeam;
                return new LastMatchResult(isWin, isDraw, opponent);
            }).Reverse().ToList();

            return new Standing
            {
                Team = team,
                P = games.Count,
                W = wins,
                D = draws,
                L = losses,
                GF = goalsFor,
                GA = goalsAgainst,
                GD = goalsFor - goalsAgainst,
                Pts = wins * 3 + draws,
                LastFiveMatches = lastFive
            };
        }

        private bool IsVictoryWin(GameResult game) => (game.HomeTeam == _teamName && (game.HomeScore ?? 0) > (game.AwayScore ?? 0)) || (game.AwayTeam == _teamName && (game.AwayScore ?? 0) > (game.HomeScore ?? 0));
        private bool IsVictoryDraw(GameResult game) => (game.HomeScore ?? 0) == (game.AwayScore ?? 0);
        private bool IsVictoryLoss(GameResult game) => !IsVictoryWin(game) && !IsVictoryDraw(game);

        private List<GameResult> LoadRegularSeasonData()
        {
            return new List<GameResult>
            {
                new() { GameNumber = 11, GameDate = new DateTime(2025, 5, 25, 11, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 2, AwayTeam = "Azzurri ST", AwayScore = 0, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 10, GameDate = new DateTime(2025, 5, 25, 9, 0, 0), Field = "Proto Field", HomeTeam = "Proto 3", HomeScore = 2, AwayTeam = "Proto 4", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 9, GameDate = new DateTime(2025, 5, 25, 11, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 4, AwayTeam = "Sons of Italy Inter", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 12, GameDate = new DateTime(2025, 5, 25, 11, 0, 0), Field = "Heritage Green 5", HomeTeam = "West Hamilton United 89", HomeScore = 0, AwayTeam = "Victory FC", AwayScore = 7, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 29, GameDate = new DateTime(2025, 6, 1, 11, 0, 0), Field = "Heritage Green 5", HomeTeam = "Proto 4", HomeScore = 0, AwayTeam = "Victory FC", AwayScore = 6, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 28, GameDate = new DateTime(2025, 6, 1, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "Azzurri ST", HomeScore = 1, AwayTeam = "Hamilton Serbian United", AwayScore = 3, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 31, GameDate = new DateTime(2025, 6, 1, 9, 0, 0), Field = "Mohawk 5", HomeTeam = "West Hamilton United 89", HomeScore = 3, AwayTeam = "United FC", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 30, GameDate = new DateTime(2025, 6, 1, 9, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 0, AwayTeam = "Proto 3", AwayScore = 2, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 47, GameDate = new DateTime(2025, 6, 8, 11, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 2, AwayTeam = "West Hamilton United 89", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 48, GameDate = new DateTime(2025, 6, 8, 11, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 6, AwayTeam = "Azzurri ST", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 50, GameDate = new DateTime(2025, 6, 8, 11, 0, 0), Field = "Mohawk 2", HomeTeam = "Victory FC", HomeScore = 5, AwayTeam = "Proto 3", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 49, GameDate = new DateTime(2025, 6, 8, 9, 0, 0), Field = "Mohawk 5", HomeTeam = "United FC", HomeScore = 2, AwayTeam = "Proto 4", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 69, GameDate = new DateTime(2025, 6, 15, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "West Hamilton United 89", HomeScore = 3, AwayTeam = "Sons of Italy Inter", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 66, GameDate = new DateTime(2025, 6, 15, 11, 0, 0), Field = "Proto Field", HomeTeam = "Proto 3", HomeScore = 7, AwayTeam = "Azzurri ST", AwayScore = 3, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 67, GameDate = new DateTime(2025, 6, 15, 9, 0, 0), Field = "Proto Field", HomeTeam = "Proto 4", HomeScore = 2, AwayTeam = "Hamilton Serbian United", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 68, GameDate = new DateTime(2025, 6, 15, 11, 0, 0), Field = "Mohawk 5", HomeTeam = "Victory FC", HomeScore = 3, AwayTeam = "United FC", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 85, GameDate = new DateTime(2025, 6, 22, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "Azzurri ST", HomeScore = 0, AwayTeam = "West Hamilton United 89", AwayScore = 3, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 87, GameDate = new DateTime(2025, 6, 22, 9, 0, 0), Field = "Mohawk 3", HomeTeam = "Sons of Italy Inter", HomeScore = 4, AwayTeam = "Proto 4", AwayScore = 2, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 88, GameDate = new DateTime(2025, 6, 22, 11, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 4, AwayTeam = "Proto 3", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 86, GameDate = new DateTime(2025, 6, 22, 9, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 0, AwayTeam = "Victory FC", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 104, GameDate = new DateTime(2025, 7, 6, 9, 0, 0), Field = "Proto Field", HomeTeam = "Proto 3", HomeScore = 2, AwayTeam = "West Hamilton United 89", AwayScore = 4, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 107, GameDate = new DateTime(2025, 7, 6, 9, 0, 0), Field = "Ancaster 3-A", HomeTeam = "Victory FC", HomeScore = 5, AwayTeam = "Sons of Italy Inter", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 105, GameDate = new DateTime(2025, 7, 6, 11, 0, 0), Field = "Proto Field", HomeTeam = "Proto 4", HomeScore = 0, AwayTeam = "Azzurri ST", AwayScore = 3, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 106, GameDate = new DateTime(2025, 7, 6, 9, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 3, AwayTeam = "United FC", AwayScore = 2, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 124, GameDate = new DateTime(2025, 7, 13, 11, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 2, AwayTeam = "Proto 3", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 126, GameDate = new DateTime(2025, 7, 13, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "West Hamilton United 89", HomeScore = 2, AwayTeam = "Proto 4", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 125, GameDate = new DateTime(2025, 7, 13, 11, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 1, AwayTeam = "United FC", AwayScore = 2, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 123, GameDate = new DateTime(2025, 7, 13, 11, 0, 0), Field = "Mohawk 6", HomeTeam = "Azzurri ST", HomeScore = 0, AwayTeam = "Victory FC", AwayScore = 6, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 143, GameDate = new DateTime(2025, 7, 20, 11, 0, 0), Field = "Proto Field", HomeTeam = "Proto 4", HomeScore = 2, AwayTeam = "Proto 3", AwayScore = 6, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 145, GameDate = new DateTime(2025, 7, 20, 9, 0, 0), Field = "Ancaster 3-A", HomeTeam = "Victory FC", HomeScore = 5, AwayTeam = "West Hamilton United 89", AwayScore = 0, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 142, GameDate = new DateTime(2025, 7, 20, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "Azzurri ST", HomeScore = 0, AwayTeam = "United FC", AwayScore = 3, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 144, GameDate = new DateTime(2025, 7, 20, 9, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 0, AwayTeam = "Hamilton Serbian United", AwayScore = 2, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 161, GameDate = new DateTime(2025, 7, 27, 9, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 1, AwayTeam = "Azzurri ST", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 163, GameDate = new DateTime(2025, 7, 27, 11, 0, 0), Field = "Heritage Green 5", HomeTeam = "United FC", HomeScore = 1, AwayTeam = "West Hamilton United 89", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 162, GameDate = new DateTime(2025, 7, 27, 9, 0, 0), Field = "Heritage Green 3", HomeTeam = "Proto 3", HomeScore = 6, AwayTeam = "Sons of Italy Inter", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 164, GameDate = new DateTime(2025, 7, 27, 11, 0, 0), Field = "Heritage Green 3", HomeTeam = "Victory FC", HomeScore = 3, AwayTeam = "Proto 4", AwayScore = 0, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 258, GameDate = new DateTime(2025, 8, 6, 19, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 0, AwayTeam = "Sons of Italy Inter", AwayScore = 3, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 183, GameDate = new DateTime(2025, 8, 10, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "West Hamilton United 89", HomeScore = 2, AwayTeam = "Hamilton Serbian United", AwayScore = 2, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 181, GameDate = new DateTime(2025, 8, 10, 11, 0, 0), Field = "Mohawk 5", HomeTeam = "Proto 3", HomeScore = 2, AwayTeam = "Victory FC", AwayScore = 2, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 180, GameDate = new DateTime(2025, 8, 10, 11, 0, 0), Field = "Heritage Green 2", HomeTeam = "Azzurri ST", HomeScore = 3, AwayTeam = "Sons of Italy Inter", AwayScore = 2, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 182, GameDate = new DateTime(2025, 8, 10, 11, 0, 0), Field = "Heritage Green 3", HomeTeam = "Proto 4", HomeScore = 0, AwayTeam = "United FC", AwayScore = 0, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 202, GameDate = new DateTime(2025, 8, 17, 9, 0, 0), Field = "Sackville", HomeTeam = "United FC", HomeScore = 0, AwayTeam = "Victory FC", AwayScore = 4, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 199, GameDate = new DateTime(2025, 8, 17, 11, 0, 0), Field = "Sackville", HomeTeam = "Azzurri ST", HomeScore = 0, AwayTeam = "Proto 3", AwayScore = 5, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 200, GameDate = new DateTime(2025, 8, 17, 11, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 0, AwayTeam = "Proto 4", AwayScore = 0, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 201, GameDate = new DateTime(2025, 8, 17, 9, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 2, AwayTeam = "West Hamilton United 89", AwayScore = 3, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 218, GameDate = new DateTime(2025, 8, 24, 9, 0, 0), Field = "Heritage Green 3", HomeTeam = "Proto 3", HomeScore = 0, AwayTeam = "United FC", AwayScore = 0, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 220, GameDate = new DateTime(2025, 8, 24, 9, 0, 0), Field = "Ancaster 3-A", HomeTeam = "Victory FC", HomeScore = 6, AwayTeam = "Hamilton Serbian United", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 221, GameDate = new DateTime(2025, 8, 24, 11, 0, 0), Field = "Heritage Green 5", HomeTeam = "West Hamilton United 89", HomeScore = 3, AwayTeam = "Azzurri ST", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 219, GameDate = new DateTime(2025, 8, 24, 9, 0, 0), Field = "Proto Field", HomeTeam = "Proto 4", HomeScore = 1, AwayTeam = "Sons of Italy Inter", AwayScore = 0, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 237, GameDate = new DateTime(2025, 9, 7, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "Azzurri ST", HomeScore = 1, AwayTeam = "Proto 4", AwayScore = 3, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 240, GameDate = new DateTime(2025, 9, 7, 11, 0, 0), Field = "Ancaster 3-A", HomeTeam = "West Hamilton United 89", HomeScore = 1, AwayTeam = "Proto 3", AwayScore = 1, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 239, GameDate = new DateTime(2025, 9, 7, 9, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 3, AwayTeam = "Victory FC", AwayScore = 0, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 238, GameDate = new DateTime(2025, 9, 7, 9, 0, 0), Field = "Mohawk 6", HomeTeam = "United FC", HomeScore = 2, AwayTeam = "Hamilton Serbian United", AwayScore = 0, IsCompleted = true, Competition = "regular"  },
                new() { GameNumber = 256, GameDate = new DateTime(2025, 9, 14, 9, 0, 0), Field = "Proto Field", HomeTeam = "Proto 3", HomeScore = null, AwayTeam = "Hamilton Serbian United", AwayScore = null, IsCompleted = false, Competition = "regular"  },
                new() { GameNumber = 257, GameDate = new DateTime(2025, 9, 14, 11, 0, 0), Field = "Proto Field", HomeTeam = "Proto 4", HomeScore = null, AwayTeam = "West Hamilton United 89", AwayScore = null, IsCompleted = false, Competition = "regular"  },
                new() { GameNumber = 259, GameDate = new DateTime(2025, 9, 14, 11, 0, 0), Field = "Heritage Green 5", HomeTeam = "Victory FC", HomeScore = null, AwayTeam = "Azzurri ST", AwayScore = null, IsCompleted = false, Competition = "regular"  }
            };
        }

        private List<GameResult> LoadSpenceCupData()
        {
            return new List<GameResult>
            {
                new() { GameNumber = 52, GameDate = new DateTime(2025, 5, 11, 11, 0, 0), Field = "Brebeuf Turf # 1", HomeTeam = "Iam Emeralds ST", HomeScore = 1, AwayTeam = "Victory FC", AwayScore = 9, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 53, GameDate = new DateTime(2025, 5, 11, 13, 0, 0), Field = "Cardinal Newman HS (turf)", HomeTeam = "Glasgow United FC", HomeScore = 1, AwayTeam = "Sons of Italy Inter", AwayScore = 2, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 21, GameDate = new DateTime(2025, 5, 11, 9, 0, 0), Field = "Cardinal Newman HS (turf)", HomeTeam = "United FC", HomeScore = 4, AwayTeam = "Proto 3", AwayScore = 3, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 22, GameDate = new DateTime(2025, 5, 11, 11, 0, 0), Field = "Cardinal Newman HS (turf)", HomeTeam = "Proto 4", HomeScore = 0, AwayTeam = "Bell City FC", AwayScore = 3, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 24, GameDate = new DateTime(2025, 5, 11, 9, 0, 0), Field = "Brebeuf Turf # 1", HomeTeam = "Hamilton Croatia Hrvat", HomeScore = 0, AwayTeam = "Hamilton Serbian United", AwayScore = 1, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 26, GameDate = new DateTime(2025, 5, 28, 21, 0, 0), Field = "Bishop Ryan HS (turf)", HomeTeam = "Stoney Creek United", HomeScore = 0, AwayTeam = "West Hamilton United 89", AwayScore = 4, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 29, GameDate = new DateTime(2025, 5, 28, 21, 0, 0), Field = "Mohawk 2", HomeTeam = "Sons of Italy Inter", HomeScore = 2, AwayTeam = "Hamilton Serbian United", AwayScore = 1, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 28, GameDate = new DateTime(2025, 5, 28, 19, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 2, AwayTeam = "Bell City FC", AwayScore = 1, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 27, GameDate = new DateTime(2025, 6, 11, 21, 0, 0), Field = "Mohawk 2", HomeTeam = "Unipol FC", HomeScore = 1, AwayTeam = "Hamilton Juventus FC", AwayScore = 4, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 30, GameDate = new DateTime(2025, 6, 11, 19, 0, 0), Field = "Mohawk 2", HomeTeam = "Victory FC", HomeScore = 9, AwayTeam = "West Hamilton United 89", AwayScore = 2, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 31, GameDate = new DateTime(2025, 6, 25, 19, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 4, AwayTeam = "Sons of Italy Inter", AwayScore = 1, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 32, GameDate = new DateTime(2025, 6, 25, 21, 0, 0), Field = "Mohawk 2", HomeTeam = "Victory FC", HomeScore = 4, AwayTeam = "Hamilton Juventus FC", AwayScore = 0, IsCompleted = true, Competition = "spence" },
                new() { GameNumber = 33, GameDate = new DateTime(2025, 9, 3, 19, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 2, AwayTeam = "Victory FC", AwayScore = 3, IsCompleted = true, Competition = "spence" }
            };
        }
    }
}