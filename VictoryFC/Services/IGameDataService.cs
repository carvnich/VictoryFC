using VictoryFC.Models;

namespace VictoryFC.Services
{
    public interface IGameDataService
    {
        Task<List<Standing>> GetCurrentStandingsAsync(string division = "regular");
        Task<GameResult> GetNextGameAsync();
        Task<List<GameResult>> GetRecentResultsAsync(int count = 5);
        Task<List<GameResult>> GetUpcomingGamesAsync(int count = 3);
        Task<SeasonStats> GetSeasonStatsAsync();
        Task<string> GetNextGameLocationAsync();
    }
}