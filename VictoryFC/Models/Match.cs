using System.ComponentModel.DataAnnotations;

namespace VictoryFC.Models
{
    public class Match
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        [Required, MaxLength(100)]
        public string Field { get; set; }

        [Required, MaxLength(50)]
        public string HomeTeam { get; set; }
        public int? HomeScore { get; set; }

        [Required, MaxLength(50)]
        public string AwayTeam { get; set; }
        public int? AwayScore { get; set; }
        public bool IsCompleted { get; set; }

        [MaxLength(20)]
        public string Competition { get; set; } = "regular";

        // Computed properties (not stored in DB)
        public string Result => IsCompleted ? $"{HomeScore} : {AwayScore}" : "vs";
        public string FormattedDate => Date.ToString("MMM dd, yyyy");
        public string FormattedTime => Date.ToString("h:mm tt");
        public string CompetitionClass => Competition == "spence" ? "spence-cup" : "regular-season";
    }
}