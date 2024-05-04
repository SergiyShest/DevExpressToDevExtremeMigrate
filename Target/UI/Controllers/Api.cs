using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class Api : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
