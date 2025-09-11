namespace VictoryFC.Models
{
    public class GameResult
    {
        public int GameNumber { get; set; }
        public DateTime GameDate { get; set; }
        public string Field { get; set; }
        public string HomeTeam { get; set; }
        public int? HomeScore { get; set; }
        public string AwayTeam { get; set; }
        public int? AwayScore { get; set; }
        public bool IsCompleted { get; set; }
        public string Competition { get; set; } = "regular"; // "regular" or "spence"
        public string Result => IsCompleted ? $"{HomeScore} : {AwayScore}" : "vs";
        public string FormattedDate => GameDate.ToString("MMM dd, yyyy");
        public string FormattedTime => GameDate.ToString("h:mm tt");
        public string CompetitionClass => Competition == "spence" ? "spence-cup" : "regular-season";
    }
}