using Microsoft.AspNetCore.Mvc;

namespace RandomPayMCSD.Controllers
{
    public class ActividadesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
