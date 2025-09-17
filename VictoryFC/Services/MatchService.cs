using Microsoft.EntityFrameworkCore;
using VictoryFC.Data;
using VictoryFC.Models;

namespace VictoryFC.Services
{
    public class MatchService : IMatchService
    {
        private readonly ApplicationDbContext _context;
        private const string TeamName = "Victory FC";

        // Field locations Dictionary
        private static readonly Dictionary<string, string> FieldLocations = new()
        {
            ["Heritage Green"] = "355 First Rd W, Stoney Creek, ON L8E 0G5",
            ["Mohawk"] = "135 Fennell Ave W, Hamilton, ON L9C 1E9",
            ["Hamilton Italian Centre"] = "1145 Stone Church Rd E, Hamilton, ON L8W 3J6",
            ["Shady Acres"] = "1500 Shaver Rd, Ancaster, ON L9G 3K9",
            ["Ancaster"] = "363 Wilson St E, Ancaster, ON L9G 2B8",
            ["Proto Field"] = "65 Herkimer St, Hamilton, ON L8P 2G5",
            ["Sackville"] = "316 Sackville Hill Lane, Lower Sackville, NS B4C 2R9"
        };

        public MatchService(ApplicationDbContext context) => _context = context;

        // All matches with filtering
        public async Task<List<Match>> GetMatchesAsync(string competition = "all", string filter = "all")
        {
            var query = _context.Matches.AsQueryable();

            if (competition == "regular")
            {
                query = query.Where(m => m.Competition == "regular" || m.Competition == "playoff");
            }
            else if (competition != "all")
            {
                query = query.Where(m => m.Competition == competition);
            }

            if (filter == "victory") query = query.Where(m => m.HomeTeam == TeamName || m.AwayTeam == TeamName);

            return await query.OrderByDescending(m => m.Date).ToListAsync();
        }

        public async Task<List<Standing>> GetStandingsAsync(string division = "regular")
        {
            var matches = await _context.Matches.Where(m => m.Competition == (division == "spence" ? "spence" : "regular")).ToListAsync();
            var teams = matches.SelectMany(m => new[] { m.HomeTeam, m.AwayTeam }).Distinct();
            var standings = teams.Select(team => CalculateStanding(team, matches))
                .OrderByDescending(s => s.Pts).ThenByDescending(s => s.GD).ThenByDescending(s => s.GF).ToList();

            for (int i = 0; i < standings.Count; i++) standings[i].Position = i + 1;
            return standings;
        }

        public async Task<Match?> GetNextMatchAsync() => await _context.Matches
            .Where(m => (m.HomeScore == null || m.AwayScore == null) && (m.HomeTeam == TeamName || m.AwayTeam == TeamName))
            .OrderBy(m => m.Date).FirstOrDefaultAsync();

        public async Task<List<Scorer>> GetTopScorersAsync(string competition = "regular") =>
            await _context.Scorers.Where(s => s.Competition == competition).OrderByDescending(s => s.Goals).ToListAsync();

        public async Task<SeasonStats> GetSeasonStatsAsync()
        {
            var matches = await _context.Matches
                .Where(m => m.HomeScore != null && m.AwayScore != null && (m.HomeTeam == TeamName || m.AwayTeam == TeamName))
                .ToListAsync();

            return new SeasonStats
            {
                Wins = matches.Count(IsWin),
                Draws = matches.Count(IsDraw),
                Losses = matches.Count(m => !IsWin(m) && !IsDraw(m)),
                GoalsFor = matches.Sum(m => m.HomeTeam == TeamName ? m.HomeScore!.Value : m.AwayScore!.Value),
                GoalsAgainst = matches.Sum(m => m.HomeTeam == TeamName ? m.AwayScore!.Value : m.HomeScore!.Value)
            };
        }

        // Score update
        public async Task UpdateMatchScoreAsync(int matchId, int homeScore, int awayScore)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match != null)
            {
                match.HomeScore = homeScore;
                match.AwayScore = awayScore;
                await _context.SaveChangesAsync();

                // Only update playoffs if this is a playoff match
                if (match.Competition == "playoff")
                {
                    await UpdatePlayoffAdvancementAsync();
                }
            }
        }

        // Playoff advancement logic
        private async Task UpdatePlayoffAdvancementAsync()
        {
            // Get completed quarterfinals
            var completedQuarterfinals = await _context.Matches
                .Where(m => m.Competition == "playoff" && m.Round == "quarterfinal" && m.HomeScore != null && m.AwayScore != null)
                .ToListAsync();

            // Get semifinals that need updating
            var semifinals = await _context.Matches
                .Where(m => m.Competition == "playoff" && m.Round == "semifinal")
                .ToListAsync();

            // Update semifinals based on quarterfinal winners
            foreach (var semi in semifinals)
            {
                if (semi.GameNumber == 5) // Winner of Game 1 vs Winner of Game 2
                {
                    semi.HomeTeam = completedQuarterfinals.FirstOrDefault(q => q.GameNumber == 1)?.WinnerTeam ?? "TBD";
                    semi.AwayTeam = completedQuarterfinals.FirstOrDefault(q => q.GameNumber == 2)?.WinnerTeam ?? "TBD";
                }
                else if (semi.GameNumber == 6) // Winner of Game 3 vs Winner of Game 4
                {
                    semi.HomeTeam = completedQuarterfinals.FirstOrDefault(q => q.GameNumber == 3)?.WinnerTeam ?? "TBD";
                    semi.AwayTeam = completedQuarterfinals.FirstOrDefault(q => q.GameNumber == 4)?.WinnerTeam ?? "TBD";
                }
            }

            // Get completed semifinals
            var completedSemifinals = await _context.Matches
                .Where(m => m.Competition == "playoff" && m.Round == "semifinal" && m.HomeScore != null && m.AwayScore != null)
                .ToListAsync();

            // Update final
            var final = await _context.Matches
                .FirstOrDefaultAsync(m => m.Competition == "playoff" && m.Round == "final");

            if (final != null)
            {
                final.HomeTeam = completedSemifinals.FirstOrDefault(s => s.GameNumber == 5)?.WinnerTeam ?? "TBD";
                final.AwayTeam = completedSemifinals.FirstOrDefault(s => s.GameNumber == 6)?.WinnerTeam ?? "TBD";
            }

            await _context.SaveChangesAsync();
        }

        public string GetLocationAddress(string? field)
        {
            if (string.IsNullOrWhiteSpace(field))
                return FieldLocations["Heritage Green"];

            var fieldLower = field.ToLowerInvariant();

            if (fieldLower.Contains("heritage green")) return FieldLocations["Heritage Green"];
            if (fieldLower.Contains("mohawk")) return FieldLocations["Mohawk"];
            if (fieldLower.Contains("italian centre")) return FieldLocations["Hamilton Italian Centre"];
            if (fieldLower.Contains("shady acres")) return FieldLocations["Shady Acres"];
            if (fieldLower.Contains("ancaster")) return FieldLocations["Ancaster"];
            if (fieldLower.Contains("proto")) return FieldLocations["Proto Field"];
            if (fieldLower.Contains("sackville")) return FieldLocations["Sackville"];

            return FieldLocations["Heritage Green"]; // Default
        }

        private bool IsWin(Match m) => (m.HomeTeam == TeamName && m.HomeScore > m.AwayScore) || (m.AwayTeam == TeamName && m.AwayScore > m.HomeScore);
        private bool IsDraw(Match m) => m.HomeScore == m.AwayScore;

        private Standing CalculateStanding(string team, List<Match> matches)
        {
            var teamMatches = matches.Where(m => (m.HomeTeam == team || m.AwayTeam == team) && m.HomeScore != null && m.AwayScore != null).ToList();
            var wins = teamMatches.Count(m => (m.HomeTeam == team && m.HomeScore > m.AwayScore) || (m.AwayTeam == team && m.AwayScore > m.HomeScore));
            var draws = teamMatches.Count(m => m.HomeScore == m.AwayScore);

            var lastFive = teamMatches.OrderByDescending(m => m.Date).Take(5)
                .Select(m => new MatchResult(
                    (m.HomeTeam == team && m.HomeScore > m.AwayScore) || (m.AwayTeam == team && m.AwayScore > m.HomeScore),
                    m.HomeScore == m.AwayScore,
                    m.HomeTeam == team ? m.AwayTeam : m.HomeTeam))
                .Reverse().ToList();

            return new Standing
            {
                Team = team,
                P = teamMatches.Count,
                W = wins,
                D = draws,
                L = teamMatches.Count - wins - draws,
                GF = teamMatches.Sum(m => m.HomeTeam == team ? m.HomeScore!.Value : m.AwayScore!.Value),
                GA = teamMatches.Sum(m => m.HomeTeam == team ? m.AwayScore!.Value : m.HomeScore!.Value),
                LastFive = lastFive
            };
        }
    }
}