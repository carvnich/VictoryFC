namespace VictoryFC.Models
{
    public class Standing
    {
        public int Position { get; set; }
        public string Team { get; set; }
        public int P { get; set; } // Games played
        public int W { get; set; } // Wins
        public int D { get; set; } // Draws  
        public int L { get; set; } // Losses
        public int GF { get; set; } // Goals for
        public int GA { get; set; } // Goals against
        public int GD => GF - GA; // Goal difference
        public int Pts => W * 3 + D; // Points
        public List<MatchResult> LastFive { get; set; } = new();
    }

    public record MatchResult(bool IsWin, bool IsDraw, string Opponent);
}