using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VictoryFC.Models;

namespace VictoryFC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var shopItems = new List<ShopItem>
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

            var standings = new List<Standing>
            {
                new()
                {
                    Position = 1,
                    Team = "Victory FC",
                    P = 12,
                    W = 11,
                    D = 1,
                    L = 0,
                    GF = 53,
                    GA = 6,
                    GD = 47,
                    Pts = 34,
                    LastFiveMatches = new List<LastMatchResult>
                    {
                        new(true, false, "West Hamilton United"),
                        new(true, false, "Proto 3"),
                        new(true, false, "Hamilton Serbian United"),
                        new(false, true, "United FC"),
                        new(true, false, "Sons of Italy Inter")
                    }
                },
                new()
                {
                    Position = 2,
                    Team = "West Hamilton United",
                    P = 8,
                    W = 9,
                    D = 1,
                    L = 3,
                    GF = 33,
                    GA = 24,
                    GD = 26,
                    Pts = 7,
                    LastFiveMatches = new List<LastMatchResult>
                    {
                        new(false, false, "Victory FC"),
                        new(true, false, "Proto 4"),
                        new(true, false, "Azzurri ST"),
                        new(false, false, "Proto 3"),
                        new(true, false, "Hamilton Serbian United")
                    }
                },
                new()
                {
                    Position = 3,
                    Team = "Proto 3",
                    P = 13,
                    W = 6,
                    D = 3,
                    L = 4,
                    GF = 36,
                    GA = 25,
                    GD = 11,
                    Pts = 21,
                    LastFiveMatches = new List<LastMatchResult>
                    {
                        new(false, false, "Victory FC"),
                        new(true, false, "United FC"),
                        new(false, true, "Hamilton Serbian United"),
                        new(true, false, "West Hamilton United"),
                        new(false, false, "Sons of Italy Inter")
                    }
                },
                new()
                {
                    Position = 4,
                    Team = "Hamilton Serbian United",
                    P = 13,
                    W = 6,
                    D = 3,
                    L = 4,
                    GF = 21,
                    GA = 20,
                    GD = 1,
                    Pts = 21,
                    LastFiveMatches = new List<LastMatchResult>
                    {
                        new(false, false, "Victory FC"),
                        new(false, true, "Proto 3"),
                        new(true, false, "Azzurri ST"),
                        new(false, false, "West Hamilton United"),
                        new(false, true, "United FC")
                    }
                },
                new()
                {
                    Position = 5,
                    Team = "United FC",
                    P = 14,
                    W = 6,
                    D = 3,
                    L = 5,
                    GF = 20,
                    GA = 20,
                    GD = 0,
                    Pts = 21,
                    LastFiveMatches = new List<LastMatchResult>
                    {
                        new(false, true, "Victory FC"),
                        new(false, false, "Proto 3"),
                        new(true, false, "Proto 4"),
                        new(false, true, "Hamilton Serbian United"),
                        new(true, false, "Azzurri ST")
                    }
                },
                new()
                {
                    Position = 6,
                    Team = "Sons of Italy Inter",
                    P = 13,
                    W = 3,
                    D = 0,
                    L = 10,
                    GF = 22,
                    GA = 34,
                    GD = -12,
                    Pts = 9,
                    LastFiveMatches = new List<LastMatchResult>
                    {
                        new(false, false, "Victory FC"),
                        new(false, false, "West Hamilton United"),
                        new(true, false, "Proto 3"),
                        new(false, false, "Hamilton Serbian United"),
                        new(false, false, "United FC")
                    }
                },
                new()
                {
                    Position = 7,
                    Team = "Proto 4",
                    P = 12,
                    W = 2,
                    D = 2,
                    L = 8,
                    GF = 10,
                    GA = 29,
                    GD = -19,
                    Pts = 8,
                    LastFiveMatches = new List<LastMatchResult>
                    {
                        new(false, false, "West Hamilton United"),
                        new(false, false, "United FC"),
                        new(false, true, "Azzurri ST"),
                        new(false, false, "Hamilton Serbian United"),
                        new(false, false, "Victory FC")
                    }
                },
                new()
                {
                    Position = 8,
                    Team = "Azzurri ST",
                    P = 12,
                    W = 2,
                    D = 1,
                    L = 9,
                    GF = 13,
                    GA = 41,
                    GD = -28,
                    Pts = 7,
                    LastFiveMatches = new List<LastMatchResult>
                    {
                        new(false, false, "West Hamilton United"),
                        new(false, false, "Hamilton Serbian United"),
                        new(false, true, "Proto 4"),
                        new(false, false, "United FC"),
                        new(false, false, "Victory FC")
                    }
                }
            };

            return View(new HomeViewModel { ShopItems = shopItems, Standings = standings });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}