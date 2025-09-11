using System.Collections.Generic;

namespace VictoryFC.Models
{
    public class ShopItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
    }

    public class Standing
    {
        public int Position { get; set; }
        public string Team { get; set; }
        public int P { get; set; }
        public int W { get; set; }
        public int D { get; set; }
        public int L { get; set; }
        public int GF { get; set; }
        public int GA { get; set; }
        public int GD { get; set; }
        public int Pts { get; set; }
        public List<LastMatchResult> LastFiveMatches { get; set; } = new List<LastMatchResult>();
    }

    public class LastMatchResult
    {
        public bool IsWin { get; set; }
        public bool IsDraw { get; set; }
        public string Opponent { get; set; }

        public LastMatchResult(bool isWin, bool isDraw, string opponent)
        {
            IsWin = isWin;
            IsDraw = isDraw;
            Opponent = opponent;
        }
    }

    public class HomeViewModel
    {
        public List<ShopItem> ShopItems { get; set; } = new List<ShopItem>();
        public List<Standing> Standings { get; set; } = new List<Standing>();
    }
}