using Microsoft.AspNetCore.Mvc;
using VictoryFC.Models;
using VictoryFC.Services;

namespace VictoryFC.Controllers
{
    public class MatchesController : Controller
    {
        private readonly IMatchService _matchService;

        public MatchesController(IMatchService matchService)
        {
            _matchService = matchService;
        }

        public async Task<IActionResult> Index()
        {
            var allMatches = await _matchService.GetAllMatchesAsync();
            var viewModel = new MatchesViewModel { Matches = allMatches };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetMatches(string competition = "all")
        {
            var matches = await _matchService.GetMatchesByCompetitionAsync(competition);
            return Json(new { matches });
        }

        [HttpGet]
        public async Task<IActionResult> GetMatchesPartial(string competition = "all", string filter = "all")
        {
            var matches = await _matchService.GetMatchesByCompetitionAsync(competition);

            // Filter for Victory FC matches if requested
            if (filter == "victory")
            {
                matches = matches.Where(m => m.HomeTeam == "Victory FC" || m.AwayTeam == "Victory FC").ToList();
            }

            var groupedMatches = matches
                .OrderByDescending(m => m.Date)
                .GroupBy(m => m.Date.Date)
                .OrderByDescending(g => g.Key);

            return PartialView("_MatchesList", groupedMatches);
        }

        [HttpGet]
        public IActionResult GetMatchRow(int id)
        {
            // You could fetch a single match here if needed
            var match = new Match(); // Get from service by id
            return PartialView("_MatchRow", match);
        }
    }
}