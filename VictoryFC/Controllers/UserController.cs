using Microsoft.AspNetCore.Mvc;

namespace VictoryFC.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
