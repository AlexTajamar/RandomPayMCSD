using Microsoft.AspNetCore.Mvc;

namespace RandomPayMCSD.Controllers
{
    public class ParticipantesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
