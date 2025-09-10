namespace VictoryFC.Models
{
    public class DynamicHomeViewModel
    {
        public List<ShopItem> ShopItems { get; set; } = new();
        public List<Standing> Standings { get; set; } = new();
        public GameResult NextGame { get; set; }
        public List<GameResult> RecentResults { get; set; } = new();
        public List<GameResult> UpcomingGames { get; set; } = new();
        public SeasonStats SeasonStats { get; set; }
        public string NextGameLocation { get; set; }
        public string TeamFormClass => GetFormClass();
        public string NextGameCountdownTarget => NextGame?.GameDate.ToString("yyyy-MM-ddTHH:mm:ss") ?? "";

        private string GetFormClass()
        {
            if (RecentResults?.Count >= 3)
            {
                var recentWins = RecentResults.Take(3).Count(r =>
                    (r.HomeTeam == "VICTORY FC" && (r.HomeScore ?? 0) > (r.AwayScore ?? 0)) ||
                    (r.AwayTeam == "VICTORY FC" && (r.AwayScore ?? 0) > (r.HomeScore ?? 0)));
                return recentWins >= 2 ? "excellent" : recentWins == 1 ? "good" : "poor";
            }
            return "neutral";
        }
    }
}