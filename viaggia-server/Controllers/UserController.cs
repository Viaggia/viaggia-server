using Microsoft.AspNetCore.Mvc;

namespace viaggia_server.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
