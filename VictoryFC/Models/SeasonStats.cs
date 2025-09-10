namespace VictoryFC.Models
{
    public class SeasonStats
    {
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }
        public double WinPercentage { get; set; }
        public int GoalDifference => GoalsFor - GoalsAgainst;
    }
}
