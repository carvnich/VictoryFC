using Microsoft.AspNetCore.Mvc;
using VictoryFC.Models;
using VictoryFC.Services;

namespace VictoryFC.Controllers
{
    public class MatchController : Controller
    {
        private readonly IMatchService _matchService;

        public MatchController(IMatchService matchService) => _matchService = matchService;

        public async Task<IActionResult> Index()
        {
            return View(new HomeViewModel
            {
                AllMatches = await _matchService.GetMatchesAsync(),
                VictoryMatches = await _matchService.GetMatchesAsync(filter: "victory")
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMatchesPartial(string competition = "all", string filter = "all")
        {
            var matches = await _matchService.GetMatchesAsync(competition, filter);
            var groupedMatches = matches.OrderByDescending(m => m.Date).GroupBy(m => m.Date.Date).OrderByDescending(g => g.Key);
            ViewData["ShowEditButtons"] = true;
            return PartialView("_MatchesList", groupedMatches);
        }
    }
}