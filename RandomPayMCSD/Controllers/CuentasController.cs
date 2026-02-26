using Microsoft.AspNetCore.Mvc;

namespace RandomPayMCSD.Controllers
{
    public class CuentasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
