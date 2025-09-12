using System.ComponentModel.DataAnnotations;

namespace VictoryFC.Models
{
    public class Scorer
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string PlayerName { get; set; }

        [Required, MaxLength(50)]
        public string TeamName { get; set; }

        public int Goals { get; set; }

        [MaxLength(20)]
        public string Competition { get; set; } = "regular"; // "regular" or "spence"
    }
}