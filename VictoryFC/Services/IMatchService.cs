using VictoryFC.Models;

namespace VictoryFC.Services
{
    public interface IMatchService
    {
        // Core data retrieval
        Task<List<Standing>> GetStandingsAsync(string division = "regular");
        Task<List<Scorer>> GetTopScorersAsync(string competition = "regular");
        Task<List<Match>> GetMatchesAsync(string competition = "all", string filter = "all");
        Task<Match?> GetNextMatchAsync();
        Task<SeasonStats> GetSeasonStatsAsync();

        // Match management
        Task UpdateMatchScoreAsync(int matchId, int homeScore, int awayScore);

        // Utility
        string GetLocationAddress(string? field);
    }
}