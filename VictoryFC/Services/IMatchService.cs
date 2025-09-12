using VictoryFC.Models;

namespace VictoryFC.Services
{
    public interface IMatchService
    {
        Task<List<Standing>> GetStandingsAsync(string division = "regular");
        Task<Match> GetNextMatchAsync();
        Task<List<Match>> GetRecentMatchesAsync(int count = 5);
        Task<List<Match>> GetAllMatchesAsync();
        Task<List<Match>> GetMatchesByCompetitionAsync(string competition = "all");
        Task<SeasonStats> GetSeasonStatsAsync();
        Task<string> GetNextMatchLocationAsync();
    }
}