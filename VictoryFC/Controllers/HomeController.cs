using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
                new() { Name = "Home Jersey", Description = "Official 2025 Season", Price = "$45.99", IconClass = "bi-shirt", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Training Top", Description = "Moisture-wicking", Price = "$32.99", IconClass = "bi-shirt", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Team Shorts", Description = "Lightweight", Price = "$24.99", IconClass = "bi-shorts", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Club Hoodie", Description = "Cool weather", Price = "$54.99", IconClass = "bi-layers", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Victory FC Scarf", Description = "Show support", Price = "$18.99", IconClass = "bi-award", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Water Bottle", Description = "Stay hydrated", Price = "$12.99", IconClass = "bi-droplet", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Cap", Description = "Sun protection", Price = "$19.99", IconClass = "bi-hat", Url = "https://manrocket.ca/collections/Victory-football-club-1" },
                new() { Name = "Gym Bag", Description = "Carry your gear", Price = "$29.99", IconClass = "bi-bag", Url = "https://manrocket.ca/collections/Victory-football-club-1" }
            };
        }
    }
}