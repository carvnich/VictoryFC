using Microsoft.EntityFrameworkCore;
using VictoryFC.Data;
using VictoryFC.Models;

namespace VictoryFC.Services
{
    public class MatchService : IMatchService
    {
        private readonly ApplicationDbContext _context;
        private const string TeamName = "Victory FC";

        public MatchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Standing>> GetStandingsAsync(string division = "regular")
        {
            var matches = await _context.Matches
                .Where(m => m.Competition == (division == "spence" ? "spence" : "regular"))
                .ToListAsync();

            var teams = matches.SelectMany(m => new[] { m.HomeTeam, m.AwayTeam }).Distinct();
            var standings = teams.Select(team => CalculateStanding(team, matches))
                .OrderByDescending(s => s.Pts)
                .ThenByDescending(s => s.GD)
                .ThenByDescending(s => s.GF)
                .ToList();

            for (int i = 0; i < standings.Count; i++) standings[i].Position = i + 1;
            return standings;
        }

        public async Task<Match> GetNextMatchAsync()
        {
            return await _context.Matches
                .Where(m => !m.IsCompleted && (m.HomeTeam == TeamName || m.AwayTeam == TeamName))
                .OrderBy(m => m.Date)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Match>> GetRecentMatchesAsync(int count = 5)
        {
            return await _context.Matches
                .Where(m => m.IsCompleted && (m.HomeTeam == TeamName || m.AwayTeam == TeamName))
                .OrderByDescending(m => m.Date)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Match>> GetAllMatchesAsync()
        {
            return await _context.Matches
                .OrderByDescending(m => m.Date)
                .ToListAsync();
        }

        public async Task<List<Match>> GetMatchesByCompetitionAsync(string competition = "all")
        {
            var query = _context.Matches.AsQueryable();

            if (competition.ToLower() != "all")
            {
                query = query.Where(m => m.Competition == (competition == "spence" ? "spence" : "regular"));
            }

            return await query.OrderByDescending(m => m.Date).ToListAsync();
        }

        public async Task<SeasonStats> GetSeasonStatsAsync()
        {
            var completedMatches = await _context.Matches
                .Where(m => m.IsCompleted && (m.HomeTeam == TeamName || m.AwayTeam == TeamName))
                .ToListAsync();

            var wins = completedMatches.Count(IsWin);
            var draws = completedMatches.Count(IsDraw);

            return new SeasonStats
            {
                GamesPlayed = completedMatches.Count,
                Wins = wins,
                Draws = draws,
                Losses = completedMatches.Count - wins - draws,
                GoalsFor = completedMatches.Sum(m => m.HomeTeam == TeamName ? (m.HomeScore ?? 0) : (m.AwayScore ?? 0)),
                GoalsAgainst = completedMatches.Sum(m => m.HomeTeam == TeamName ? (m.AwayScore ?? 0) : (m.HomeScore ?? 0))
            };
        }

        public async Task<List<Scorer>> GetTopScorersAsync(string competition = "regular", int count = 10)
        {
            return await _context.Scorers
                .Where(s => s.Competition == competition)
                .OrderByDescending(s => s.Goals)
                .Take(count)
                .ToListAsync();
        }

        public async Task<string> GetNextMatchLocationAsync()
        {
            var nextMatch = await GetNextMatchAsync();
            return GetLocationAddress(nextMatch?.Field);
        }

        // Helper methods
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
    }
}
