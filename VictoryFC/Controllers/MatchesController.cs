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
    }
}