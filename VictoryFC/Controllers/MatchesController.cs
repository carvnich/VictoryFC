using Microsoft.AspNetCore.Mvc;
using VictoryFC.Models;
using VictoryFC.Services;

namespace VictoryFC.Controllers
{
    public class MatchesController : Controller
    {
        private readonly IGameDataService _gameDataService;

        public MatchesController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task<IActionResult> Index()
        {
            var allMatches = await _gameDataService.GetAllMatchesAsync();
            var viewModel = new MatchesViewModel { Matches = allMatches };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetMatches(string competition = "all")
        {
            var matches = await _gameDataService.GetMatchesByCompetitionAsync(competition);
            return Json(new { matches });
        }

        // Return partial view HTML
        [HttpGet]
        public async Task<IActionResult> GetMatchesPartial(string competition = "all")
        {
            var matches = await _gameDataService.GetMatchesByCompetitionAsync(competition);
            var groupedMatches = matches
                .OrderByDescending(m => m.GameDate)
                .GroupBy(m => m.GameDate.Date)
                .OrderByDescending(g => g.Key);

            return PartialView("_MatchesList", groupedMatches);
        }

        // Return a single match row
        [HttpGet]
        public IActionResult GetMatchRow(int gameNumber)
        {
            // You could fetch a single match here if needed
            // For now, this is just an example structure
            var match = new GameResult(); // Get from service
            return PartialView("_MatchRow", match);
        }
    }
}