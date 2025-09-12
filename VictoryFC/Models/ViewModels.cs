namespace VictoryFC.Models
{
    public class HomeViewModel
    {
        public List<ShopItem> ShopItems { get; set; } = new();
        public List<Standing> Standings { get; set; } = new();
        public Match NextMatch { get; set; }
        public SeasonStats Stats { get; set; }
        public string NextMatchLocation { get; set; }
        public string FormClass => GetFormClass();
        public string CountdownTarget => NextMatch?.Date.ToString("yyyy-MM-ddTHH:mm:ss") ?? "";

        private string GetFormClass()
        {
            // Simplified form class logic based on recent results
            return "neutral"; // You can implement this based on your needs
        }
    }

    public class MatchesViewModel
    {
        public List<Match> Matches { get; set; } = new();
    }

    public class SeasonStats
    {
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points => Wins * 3 + Draws;
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public double WinPercentage => GamesPlayed > 0 ? (double)Wins / GamesPlayed * 100 : 0;
    }

    public class ShopItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
    }
}