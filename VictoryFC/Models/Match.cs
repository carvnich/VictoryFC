namespace VictoryFC.Models
{
    public class Match
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Field { get; set; }
        public string HomeTeam { get; set; }
        public int? HomeScore { get; set; }
        public string AwayTeam { get; set; }
        public int? AwayScore { get; set; }
        public bool IsCompleted { get; set; }
        public string Competition { get; set; } = "regular"; // "regular" or "spence"

        // Computed properties
        public string Result => IsCompleted ? $"{HomeScore} : {AwayScore}" : "vs";
        public string FormattedDate => Date.ToString("MMM dd, yyyy");
        public string FormattedTime => Date.ToString("h:mm tt");
        public string CompetitionClass => Competition == "spence" ? "spence-cup" : "regular-season";
    }
}