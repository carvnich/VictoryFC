using Microsoft.AspNetCore.Mvc;
using VictoryFC.Models;
using VictoryFC.Services;

namespace VictoryFC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMatchService _matchService;

        public HomeController(IMatchService matchService) => _matchService = matchService;

        public async Task<IActionResult> Index()
        {
            var nextMatch = await _matchService.GetNextMatchAsync();

            return View(new HomeViewModel
            {
                ShopItems = GetShopItems(),
                Standings = await _matchService.GetStandingsAsync("regular"),
                NextMatch = nextMatch,
                Stats = await _matchService.GetSeasonStatsAsync(),
                NextMatchLocation = _matchService.GetLocationAddress(nextMatch?.Field),
                AllMatches = await _matchService.GetMatchesAsync(),
                VictoryMatches = await _matchService.GetMatchesAsync(filter: "victory"),
                RegularScorers = await _matchService.GetTopScorersAsync("regular"),
                SpenceScorers = await _matchService.GetTopScorersAsync("spence")
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPartial(string type, string value = "", string filter = "")
        {
            return type switch
            {
                "standings" => PartialView("_StandingsTable", await _matchService.GetStandingsAsync(value)),
                "scorers" => PartialView("_ScorersList", await _matchService.GetTopScorersAsync(value)),
                "matches" => PartialView("_MatchesList",
                    (await _matchService.GetMatchesAsync(value, filter))
                    .OrderByDescending(m => m.Date)
                    .GroupBy(m => m.Date.Date)
                    .OrderByDescending(g => g.Key)),
                _ => BadRequest()
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetStandingsPartial(string division = "regular") =>
            PartialView("_StandingsTable", await _matchService.GetStandingsAsync(division));

        [HttpGet]
        public async Task<IActionResult> GetScorersPartial(string competition = "regular") =>
            PartialView("_ScorersList", await _matchService.GetTopScorersAsync(competition));

        [HttpGet]
        public async Task<IActionResult> GetMatchesPartial(string competition = "all", string filter = "all")
        {
            var matches = await _matchService.GetMatchesAsync(competition, filter);
            var groupedMatches = matches.OrderByDescending(m => m.Date).GroupBy(m => m.Date.Date).OrderByDescending(g => g.Key);
            return PartialView("_MatchesList", groupedMatches);
        }

        private static List<ShopItem> GetShopItems() => new()
        {
            new() { Name = "Victory FC T-Shirt", Description = "Adult", Price = "$24.99", Image = "/images/gear/tshirt_switch.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
            new() { Name = "Victory FC T-Shirt", Description = "Youth", Price = "$16.99", Image = "/images/gear/tshirt_front_logo.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
            new() { Name = "Victory FC T-Shirt", Description = "Adult", Price = "$20.00", Image = "/images/gear/tshirt_back_logo.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
            new() { Name = "Victory FC Hoodie", Description = "Adult", Price = "$49.99", Image = "/images/gear/hoodie.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
            new() { Name = "Victory FC Hoodie", Description = "Youth", Price = "$34.99", Image = "/images/gear/hoodie_youth.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
            new() { Name = "Victory FC Hat", Description = "Snapback", Price = "$24.99", Image = "/images/gear/hat.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
            new() { Name = "Victory FC Long Sleeve", Description = "Dri-Fit", Price = "$29.99", Image = "/images/gear/long_sleeve.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
            new() { Name = "Victory FC Toque", Description = "VFC Nike", Price = "$29.99", Image = "/images/gear/toque.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" }
        };
    }
}