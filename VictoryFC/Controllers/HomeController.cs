using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VictoryFC.Models;
using VictoryFC.Services;

namespace VictoryFC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGameDataService _gameDataService;

        public HomeController(ILogger<HomeController> logger, IGameDataService gameDataService)
        {
            _logger = logger;
            _gameDataService = gameDataService;
        }

        public async Task<IActionResult> Index()
        {
            var shopItems = GetShopItems();
            var standings = await _gameDataService.GetCurrentStandingsAsync("regular");
            var nextGame = await _gameDataService.GetNextGameAsync();
            var recentResults = await _gameDataService.GetRecentResultsAsync();
            var upcomingGames = await _gameDataService.GetUpcomingGamesAsync();
            var seasonStats = await _gameDataService.GetSeasonStatsAsync();
            var nextGameLocation = await _gameDataService.GetNextGameLocationAsync();

            var viewModel = new DynamicHomeViewModel
            {
                ShopItems = shopItems,
                Standings = standings,
                NextGame = nextGame,
                RecentResults = recentResults,
                UpcomingGames = upcomingGames,
                SeasonStats = seasonStats,
                NextGameLocation = nextGameLocation
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetStandings(string division = "regular")
        {
            var standings = await _gameDataService.GetCurrentStandingsAsync(division);
            return Json(new { standings });
        }

        // Return standings partial view
        [HttpGet]
        public async Task<IActionResult> GetStandingsPartial(string division = "regular")
        {
            var standings = await _gameDataService.GetCurrentStandingsAsync(division);
            return PartialView("_StandingsTable", standings);
        }

        [HttpGet]
        public async Task<IActionResult> GetLiveData()
        {
            var standings = await _gameDataService.GetCurrentStandingsAsync("regular");
            var nextGame = await _gameDataService.GetNextGameAsync();
            var seasonStats = await _gameDataService.GetSeasonStatsAsync();

            return Json(new { standings, nextGame, seasonStats });
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        private List<ShopItem> GetShopItems()
        {
            return new List<ShopItem>
            {
                new() { Name = "Victory FC T-Shirt", Description = "Adult", Price = "$24.99", Image = "/images/gear/tshirt_switch.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Victory FC T-Shirt", Description = "Youth", Price = "$16.99", Image = "/images/gear/tshirt_front_logo.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Victory FC T-Shirt", Description = "Adult", Price = "$20.99", Image = "/images/gear/tshirt_back_logo.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Victory FC Hoodie", Description = "Adult", Price = "$49.99", Image = "/images/gear/hoodie.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Victory FC Hoodie", Description = "Youth", Price = "$34.99", Image = "/images/gear/hoodie_youth.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Victory FC Hat", Description = "Snapback", Price = "$24.99", Image = "/images/gear/hat.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Victory FC Long Sleeve", Description = "Dri-Fit", Price = "$29.99", Image = "/images/gear/long_sleeve.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Victory FC Toque", Description = "VFC Nike", Price = "$29.99", Image = "/images/gear/toque.png", Url = "https://manrocket.ca/collections/Victory-football-club-1" }
            };
        }
    }
}