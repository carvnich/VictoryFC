using System.ComponentModel.DataAnnotations;

namespace VictoryFC.Models
{
    public class Match
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [Required, MaxLength(100)] public string Field { get; set; }
        [Required, MaxLength(50)] public string HomeTeam { get; set; }
        public int? HomeScore { get; set; }
        [Required, MaxLength(50)] public string AwayTeam { get; set; }
        public int? AwayScore { get; set; }
        [MaxLength(20)] public string Competition { get; set; } = "regular"; // regular, spence, playoff
        [MaxLength(50)] public string? Round { get; set; } // quarterfinal, semifinal, final
        public int? GameNumber { get; set; } // For tracking playoff games

        // Computed properties
        public bool IsCompleted => HomeScore.HasValue && AwayScore.HasValue;
        public string FormattedDate => Date.ToString("MMM dd, yyyy");
        public string FormattedTime => Date.ToString("h:mm tt");
        public string CompetitionClass => Competition switch
        {
            "spence" => "spence-cup",
            "playoff" => "playoff",
            _ => "regular-season"
        };
        public string? WinnerTeam => IsCompleted && HomeScore != AwayScore ? (HomeScore > AwayScore ? HomeTeam : AwayTeam) : null;
    }
}