namespace VictoryFC.Models
{
    public class MatchesListViewModel
    {
        public IEnumerable<IGrouping<DateTime, GameResult>> GroupedMatches { get; set; }
        public string FilterType { get; set; } = "all"; // all, victory, etc.
    }
}