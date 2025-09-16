namespace VictoryFC.Models
{
    public class SeasonStats
    {
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points => Wins * 3 + Draws;
        public int GoalDifference => GoalsFor - GoalsAgainst;
    }

    public class ShopItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
    }

    public class HomeViewModel
    {
        public List<ShopItem> ShopItems { get; set; } = new();
        public List<Standing> Standings { get; set; } = new();
        public Match? NextMatch { get; set; }
        public SeasonStats Stats { get; set; } = new();
        public string NextMatchLocation { get; set; } = "";
        public List<Match> AllMatches { get; set; } = new();
        public List<Match> VictoryMatches { get; set; } = new();
        public List<Scorer> RegularScorers { get; set; } = new();
        public List<Scorer> SpenceScorers { get; set; } = new();
        public string CountdownTarget => NextMatch?.Date.ToString("yyyy-MM-ddTHH:mm:ss") ?? "";
    }
}