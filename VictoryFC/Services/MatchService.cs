using VictoryFC.Models;

namespace VictoryFC.Services
{
    public class MatchService : IMatchService
    {
        private readonly List<Match> _allMatches;
        private const string TeamName = "Victory FC";

        public MatchService()
        {
            _allMatches = LoadMatchData();
        }

        public Task<List<Standing>> GetStandingsAsync(string division = "regular")
        {
            var matches = GetMatchesByDivision(division);
            var teams = matches.SelectMany(m => new[] { m.HomeTeam, m.AwayTeam }).Distinct();
            var standings = teams.Select(team => CalculateStanding(team, matches)).OrderByDescending(s => s.Pts).ThenByDescending(s => s.GD).ThenByDescending(s => s.GF).ToList();

            for (int i = 0; i < standings.Count; i++) standings[i].Position = i + 1;
            return Task.FromResult(standings);
        }

        public Task<Match> GetNextMatchAsync()
        {
            var next = _allMatches.Where(m => !m.IsCompleted && IsTeamMatch(m)).OrderBy(m => m.Date).FirstOrDefault();
            return Task.FromResult(next);
        }

        public Task<List<Match>> GetRecentMatchesAsync(int count = 5)
        {
            var recent = _allMatches.Where(m => m.IsCompleted && IsTeamMatch(m)).OrderByDescending(m => m.Date).Take(count).ToList();
            return Task.FromResult(recent);
        }

        public Task<List<Match>> GetAllMatchesAsync() => Task.FromResult(_allMatches);

        public Task<List<Match>> GetMatchesByCompetitionAsync(string competition = "all")
        {
            var matches = competition.ToLower() switch
            {
                "regular" => _allMatches.Where(m => m.Competition == "regular").ToList(),
                "spence" => _allMatches.Where(m => m.Competition == "spence").ToList(),
                _ => _allMatches
            };
            return Task.FromResult(matches.ToList());
        }

        public Task<SeasonStats> GetSeasonStatsAsync()
        {
            var completed = _allMatches.Where(m => m.IsCompleted && IsTeamMatch(m)).ToList();
            var wins = completed.Count(IsWin);
            var draws = completed.Count(IsDraw);

            return Task.FromResult(new SeasonStats
            {
                GamesPlayed = completed.Count,
                Wins = wins,
                Draws = draws,
                Losses = completed.Count - wins - draws,
                GoalsFor = completed.Sum(m => m.HomeTeam == TeamName ? (m.HomeScore ?? 0) : (m.AwayScore ?? 0)),
                GoalsAgainst = completed.Sum(m => m.HomeTeam == TeamName ? (m.AwayScore ?? 0) : (m.HomeScore ?? 0))
            });
        }

        public Task<string> GetNextMatchLocationAsync()
        {
            var next = GetNextMatchAsync().Result;
            return Task.FromResult(GetLocationAddress(next?.Field));
        }

        private List<Match> GetMatchesByDivision(string division) => _allMatches.Where(m => m.Competition == (division == "spence" ? "spence" : "regular")).ToList();
        private bool IsTeamMatch(Match m) => m.HomeTeam == TeamName || m.AwayTeam == TeamName;
        private bool IsWin(Match m) => (m.HomeTeam == TeamName && (m.HomeScore ?? 0) > (m.AwayScore ?? 0)) || (m.AwayTeam == TeamName && (m.AwayScore ?? 0) > (m.HomeScore ?? 0));
        private bool IsDraw(Match m) => (m.HomeScore ?? 0) == (m.AwayScore ?? 0);

        private Standing CalculateStanding(string team, List<Match> matches)
        {
            var teamMatches = matches.Where(m => (m.HomeTeam == team || m.AwayTeam == team) && m.IsCompleted).ToList();
            var wins = teamMatches.Count(m => (m.HomeTeam == team && (m.HomeScore ?? 0) > (m.AwayScore ?? 0)) || (m.AwayTeam == team && (m.AwayScore ?? 0) > (m.HomeScore ?? 0)));
            var draws = teamMatches.Count(m => (m.HomeScore ?? 0) == (m.AwayScore ?? 0));

            var lastFive = teamMatches.OrderByDescending(m => m.Date).Take(5).Select(m =>
                new MatchResult(
                    (m.HomeTeam == team && (m.HomeScore ?? 0) > (m.AwayScore ?? 0)) || (m.AwayTeam == team && (m.AwayScore ?? 0) > (m.HomeScore ?? 0)),
                    (m.HomeScore ?? 0) == (m.AwayScore ?? 0),
                    m.HomeTeam == team ? m.AwayTeam : m.HomeTeam
                )).Reverse().ToList();

            return new Standing
            {
                Team = team,
                P = teamMatches.Count,
                W = wins,
                D = draws,
                L = teamMatches.Count - wins - draws,
                GF = teamMatches.Sum(m => m.HomeTeam == team ? (m.HomeScore ?? 0) : (m.AwayScore ?? 0)),
                GA = teamMatches.Sum(m => m.HomeTeam == team ? (m.AwayScore ?? 0) : (m.HomeScore ?? 0)),
                LastFive = lastFive
            };
        }

        private string GetLocationAddress(string field) => field switch
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

        private List<Match> LoadMatchData()
        {
            return new List<Match>
            {
                // Regular Season Matches
                new() { Id = 11, Date = new DateTime(2025, 5, 25, 11, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 2, AwayTeam = "Azzurri ST", AwayScore = 0, IsCompleted = true, Competition = "regular" },
                new() { Id = 10, Date = new DateTime(2025, 5, 25, 9, 0, 0), Field = "Proto Field", HomeTeam = "Proto 3", HomeScore = 2, AwayTeam = "Proto 4", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 9, Date = new DateTime(2025, 5, 25, 11, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 4, AwayTeam = "Sons of Italy Inter", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 12, Date = new DateTime(2025, 5, 25, 11, 0, 0), Field = "Heritage Green 5", HomeTeam = "West Hamilton United 89", HomeScore = 0, AwayTeam = "Victory FC", AwayScore = 7, IsCompleted = true, Competition = "regular" },
                new() { Id = 29, Date = new DateTime(2025, 6, 1, 11, 0, 0), Field = "Heritage Green 5", HomeTeam = "Proto 4", HomeScore = 0, AwayTeam = "Victory FC", AwayScore = 6, IsCompleted = true, Competition = "regular" },
                new() { Id = 28, Date = new DateTime(2025, 6, 1, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "Azzurri ST", HomeScore = 1, AwayTeam = "Hamilton Serbian United", AwayScore = 3, IsCompleted = true, Competition = "regular" },
                new() { Id = 31, Date = new DateTime(2025, 6, 1, 9, 0, 0), Field = "Mohawk 5", HomeTeam = "West Hamilton United 89", HomeScore = 3, AwayTeam = "United FC", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 30, Date = new DateTime(2025, 6, 1, 9, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 0, AwayTeam = "Proto 3", AwayScore = 2, IsCompleted = true, Competition = "regular" },
                new() { Id = 47, Date = new DateTime(2025, 6, 8, 11, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 2, AwayTeam = "West Hamilton United 89", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 48, Date = new DateTime(2025, 6, 8, 11, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 6, AwayTeam = "Azzurri ST", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 50, Date = new DateTime(2025, 6, 8, 11, 0, 0), Field = "Mohawk 2", HomeTeam = "Victory FC", HomeScore = 5, AwayTeam = "Proto 3", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 49, Date = new DateTime(2025, 6, 8, 9, 0, 0), Field = "Mohawk 5", HomeTeam = "United FC", HomeScore = 2, AwayTeam = "Proto 4", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 69, Date = new DateTime(2025, 6, 15, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "West Hamilton United 89", HomeScore = 3, AwayTeam = "Sons of Italy Inter", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 66, Date = new DateTime(2025, 6, 15, 11, 0, 0), Field = "Proto Field", HomeTeam = "Proto 3", HomeScore = 7, AwayTeam = "Azzurri ST", AwayScore = 3, IsCompleted = true, Competition = "regular" },
                new() { Id = 67, Date = new DateTime(2025, 6, 15, 9, 0, 0), Field = "Proto Field", HomeTeam = "Proto 4", HomeScore = 2, AwayTeam = "Hamilton Serbian United", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 68, Date = new DateTime(2025, 6, 15, 11, 0, 0), Field = "Mohawk 5", HomeTeam = "Victory FC", HomeScore = 3, AwayTeam = "United FC", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 85, Date = new DateTime(2025, 6, 22, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "Azzurri ST", HomeScore = 0, AwayTeam = "West Hamilton United 89", AwayScore = 3, IsCompleted = true, Competition = "regular" },
                new() { Id = 87, Date = new DateTime(2025, 6, 22, 9, 0, 0), Field = "Mohawk 3", HomeTeam = "Sons of Italy Inter", HomeScore = 4, AwayTeam = "Proto 4", AwayScore = 2, IsCompleted = true, Competition = "regular" },
                new() { Id = 88, Date = new DateTime(2025, 6, 22, 11, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 4, AwayTeam = "Proto 3", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 86, Date = new DateTime(2025, 6, 22, 9, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 0, AwayTeam = "Victory FC", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 104, Date = new DateTime(2025, 7, 6, 9, 0, 0), Field = "Proto Field", HomeTeam = "Proto 3", HomeScore = 2, AwayTeam = "West Hamilton United 89", AwayScore = 4, IsCompleted = true, Competition = "regular" },
                new() { Id = 107, Date = new DateTime(2025, 7, 6, 9, 0, 0), Field = "Ancaster 3-A", HomeTeam = "Victory FC", HomeScore = 5, AwayTeam = "Sons of Italy Inter", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 105, Date = new DateTime(2025, 7, 6, 11, 0, 0), Field = "Proto Field", HomeTeam = "Proto 4", HomeScore = 0, AwayTeam = "Azzurri ST", AwayScore = 3, IsCompleted = true, Competition = "regular" },
                new() { Id = 106, Date = new DateTime(2025, 7, 6, 9, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 3, AwayTeam = "United FC", AwayScore = 2, IsCompleted = true, Competition = "regular" },
                new() { Id = 124, Date = new DateTime(2025, 7, 13, 11, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 2, AwayTeam = "Proto 3", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 126, Date = new DateTime(2025, 7, 13, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "West Hamilton United 89", HomeScore = 2, AwayTeam = "Proto 4", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 125, Date = new DateTime(2025, 7, 13, 11, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 1, AwayTeam = "United FC", AwayScore = 2, IsCompleted = true, Competition = "regular" },
                new() { Id = 123, Date = new DateTime(2025, 7, 13, 11, 0, 0), Field = "Mohawk 6", HomeTeam = "Azzurri ST", HomeScore = 0, AwayTeam = "Victory FC", AwayScore = 6, IsCompleted = true, Competition = "regular" },
                new() { Id = 143, Date = new DateTime(2025, 7, 20, 11, 0, 0), Field = "Proto Field", HomeTeam = "Proto 4", HomeScore = 2, AwayTeam = "Proto 3", AwayScore = 6, IsCompleted = true, Competition = "regular" },
                new() { Id = 145, Date = new DateTime(2025, 7, 20, 9, 0, 0), Field = "Ancaster 3-A", HomeTeam = "Victory FC", HomeScore = 5, AwayTeam = "West Hamilton United 89", AwayScore = 0, IsCompleted = true, Competition = "regular" },
                new() { Id = 142, Date = new DateTime(2025, 7, 20, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "Azzurri ST", HomeScore = 0, AwayTeam = "United FC", AwayScore = 3, IsCompleted = true, Competition = "regular" },
                new() { Id = 144, Date = new DateTime(2025, 7, 20, 9, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 0, AwayTeam = "Hamilton Serbian United", AwayScore = 2, IsCompleted = true, Competition = "regular" },
                new() { Id = 161, Date = new DateTime(2025, 7, 27, 9, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 1, AwayTeam = "Azzurri ST", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 163, Date = new DateTime(2025, 7, 27, 11, 0, 0), Field = "Heritage Green 5", HomeTeam = "United FC", HomeScore = 1, AwayTeam = "West Hamilton United 89", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 162, Date = new DateTime(2025, 7, 27, 9, 0, 0), Field = "Heritage Green 3", HomeTeam = "Proto 3", HomeScore = 6, AwayTeam = "Sons of Italy Inter", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 164, Date = new DateTime(2025, 7, 27, 11, 0, 0), Field = "Heritage Green 3", HomeTeam = "Victory FC", HomeScore = 3, AwayTeam = "Proto 4", AwayScore = 0, IsCompleted = true, Competition = "regular" },
                new() { Id = 258, Date = new DateTime(2025, 8, 6, 19, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 0, AwayTeam = "Sons of Italy Inter", AwayScore = 3, IsCompleted = true, Competition = "regular" },
                new() { Id = 183, Date = new DateTime(2025, 8, 10, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "West Hamilton United 89", HomeScore = 2, AwayTeam = "Hamilton Serbian United", AwayScore = 2, IsCompleted = true, Competition = "regular" },
                new() { Id = 181, Date = new DateTime(2025, 8, 10, 11, 0, 0), Field = "Mohawk 5", HomeTeam = "Proto 3", HomeScore = 2, AwayTeam = "Victory FC", AwayScore = 2, IsCompleted = true, Competition = "regular" },
                new() { Id = 180, Date = new DateTime(2025, 8, 10, 11, 0, 0), Field = "Heritage Green 2", HomeTeam = "Azzurri ST", HomeScore = 3, AwayTeam = "Sons of Italy Inter", AwayScore = 2, IsCompleted = true, Competition = "regular" },
                new() { Id = 182, Date = new DateTime(2025, 8, 10, 11, 0, 0), Field = "Heritage Green 3", HomeTeam = "Proto 4", HomeScore = 0, AwayTeam = "United FC", AwayScore = 0, IsCompleted = true, Competition = "regular" },
                new() { Id = 202, Date = new DateTime(2025, 8, 17, 9, 0, 0), Field = "Sackville", HomeTeam = "United FC", HomeScore = 0, AwayTeam = "Victory FC", AwayScore = 4, IsCompleted = true, Competition = "regular" },
                new() { Id = 199, Date = new DateTime(2025, 8, 17, 11, 0, 0), Field = "Sackville", HomeTeam = "Azzurri ST", HomeScore = 0, AwayTeam = "Proto 3", AwayScore = 5, IsCompleted = true, Competition = "regular" },
                new() { Id = 200, Date = new DateTime(2025, 8, 17, 11, 0, 0), Field = "Shady Acres 1", HomeTeam = "Hamilton Serbian United", HomeScore = 0, AwayTeam = "Proto 4", AwayScore = 0, IsCompleted = true, Competition = "regular" },
                new() { Id = 201, Date = new DateTime(2025, 8, 17, 9, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 2, AwayTeam = "West Hamilton United 89", AwayScore = 3, IsCompleted = true, Competition = "regular" },
                new() { Id = 218, Date = new DateTime(2025, 8, 24, 9, 0, 0), Field = "Heritage Green 3", HomeTeam = "Proto 3", HomeScore = 0, AwayTeam = "United FC", AwayScore = 0, IsCompleted = true, Competition = "regular" },
                new() { Id = 220, Date = new DateTime(2025, 8, 24, 9, 0, 0), Field = "Ancaster 3-A", HomeTeam = "Victory FC", HomeScore = 6, AwayTeam = "Hamilton Serbian United", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 221, Date = new DateTime(2025, 8, 24, 11, 0, 0), Field = "Heritage Green 5", HomeTeam = "West Hamilton United 89", HomeScore = 3, AwayTeam = "Azzurri ST", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 219, Date = new DateTime(2025, 8, 24, 9, 0, 0), Field = "Proto Field", HomeTeam = "Proto 4", HomeScore = 1, AwayTeam = "Sons of Italy Inter", AwayScore = 0, IsCompleted = true, Competition = "regular" },
                new() { Id = 237, Date = new DateTime(2025, 9, 7, 9, 0, 0), Field = "Heritage Green 5", HomeTeam = "Azzurri ST", HomeScore = 1, AwayTeam = "Proto 4", AwayScore = 3, IsCompleted = true, Competition = "regular" },
                new() { Id = 240, Date = new DateTime(2025, 9, 7, 11, 0, 0), Field = "Ancaster 3-A", HomeTeam = "West Hamilton United 89", HomeScore = 1, AwayTeam = "Proto 3", AwayScore = 1, IsCompleted = true, Competition = "regular" },
                new() { Id = 239, Date = new DateTime(2025, 9, 7, 9, 0, 0), Field = "Hamilton Italian Centre", HomeTeam = "Sons of Italy Inter", HomeScore = 3, AwayTeam = "Victory FC", AwayScore = 0, IsCompleted = true, Competition = "regular" },
                new() { Id = 238, Date = new DateTime(2025, 9, 7, 9, 0, 0), Field = "Mohawk 6", HomeTeam = "United FC", HomeScore = 2, AwayTeam = "Hamilton Serbian United", AwayScore = 0, IsCompleted = true, Competition = "regular" },
                new() { Id = 256, Date = new DateTime(2025, 9, 14, 9, 0, 0), Field = "Proto Field", HomeTeam = "Proto 3", HomeScore = null, AwayTeam = "Hamilton Serbian United", AwayScore = null, IsCompleted = false, Competition = "regular" },
                new() { Id = 257, Date = new DateTime(2025, 9, 14, 11, 0, 0), Field = "Proto Field", HomeTeam = "Proto 4", HomeScore = null, AwayTeam = "West Hamilton United 89", AwayScore = null, IsCompleted = false, Competition = "regular" },
                new() { Id = 259, Date = new DateTime(2025, 9, 14, 11, 0, 0), Field = "Heritage Green 5", HomeTeam = "Victory FC", HomeScore = null, AwayTeam = "Azzurri ST", AwayScore = null, IsCompleted = false, Competition = "regular" },

                // Spence Cup Matches
                new() { Id = 1052, Date = new DateTime(2025, 5, 11, 11, 0, 0), Field = "Brebeuf Turf # 1", HomeTeam = "Iam Emeralds ST", HomeScore = 1, AwayTeam = "Victory FC", AwayScore = 9, IsCompleted = true, Competition = "spence" },
                new() { Id = 1053, Date = new DateTime(2025, 5, 11, 13, 0, 0), Field = "Cardinal Newman HS (turf)", HomeTeam = "Glasgow United FC", HomeScore = 1, AwayTeam = "Sons of Italy Inter", AwayScore = 2, IsCompleted = true, Competition = "spence" },
                new() { Id = 1021, Date = new DateTime(2025, 5, 11, 9, 0, 0), Field = "Cardinal Newman HS (turf)", HomeTeam = "United FC", HomeScore = 4, AwayTeam = "Proto 3", AwayScore = 3, IsCompleted = true, Competition = "spence" },
                new() { Id = 1022, Date = new DateTime(2025, 5, 11, 11, 0, 0), Field = "Cardinal Newman HS (turf)", HomeTeam = "Proto 4", HomeScore = 0, AwayTeam = "Bell City FC", AwayScore = 3, IsCompleted = true, Competition = "spence" },
                new() { Id = 1024, Date = new DateTime(2025, 5, 11, 9, 0, 0), Field = "Brebeuf Turf # 1", HomeTeam = "Hamilton Croatia Hrvat", HomeScore = 0, AwayTeam = "Hamilton Serbian United", AwayScore = 1, IsCompleted = true, Competition = "spence" },
                new() { Id = 1026, Date = new DateTime(2025, 5, 28, 21, 0, 0), Field = "Bishop Ryan HS (turf)", HomeTeam = "Stoney Creek United", HomeScore = 0, AwayTeam = "West Hamilton United 89", AwayScore = 4, IsCompleted = true, Competition = "spence" },
                new() { Id = 1029, Date = new DateTime(2025, 5, 28, 21, 0, 0), Field = "Mohawk 2", HomeTeam = "Sons of Italy Inter", HomeScore = 2, AwayTeam = "Hamilton Serbian United", AwayScore = 1, IsCompleted = true, Competition = "spence" },
                new() { Id = 1028, Date = new DateTime(2025, 5, 28, 19, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 2, AwayTeam = "Bell City FC", AwayScore = 1, IsCompleted = true, Competition = "spence" },
                new() { Id = 1027, Date = new DateTime(2025, 6, 11, 21, 0, 0), Field = "Mohawk 2", HomeTeam = "Unipol FC", HomeScore = 1, AwayTeam = "Hamilton Juventus FC", AwayScore = 4, IsCompleted = true, Competition = "spence" },
                new() { Id = 1030, Date = new DateTime(2025, 6, 11, 19, 0, 0), Field = "Mohawk 2", HomeTeam = "Victory FC", HomeScore = 9, AwayTeam = "West Hamilton United 89", AwayScore = 2, IsCompleted = true, Competition = "spence" },
                new() { Id = 1031, Date = new DateTime(2025, 6, 25, 19, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 4, AwayTeam = "Sons of Italy Inter", AwayScore = 1, IsCompleted = true, Competition = "spence" },
                new() { Id = 1032, Date = new DateTime(2025, 6, 25, 21, 0, 0), Field = "Mohawk 2", HomeTeam = "Victory FC", HomeScore = 4, AwayTeam = "Hamilton Juventus FC", AwayScore = 0, IsCompleted = true, Competition = "spence" },
                new() { Id = 1033, Date = new DateTime(2025, 9, 3, 19, 0, 0), Field = "Mohawk 2", HomeTeam = "United FC", HomeScore = 2, AwayTeam = "Victory FC", AwayScore = 3, IsCompleted = true, Competition = "spence" }
            };
        }
    }
}