using Microsoft.AspNetCore.Http; // Necesario para la Sesión
using Microsoft.AspNetCore.Mvc;
using RandomPayMCSD.Extensions;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories;
using System.Threading.Tasks;

namespace RandomPayMCSD.Controllers
{
    public class RandomLogInController : Controller
    {
        private RepositoryUsuarios repo;

        public RandomLogInController(RepositoryUsuarios repo)
        {
            this.repo = repo;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            Usuario usuario = await this.repo.GetEmailAsync(email);

            if (usuario != null && usuario.PASSWORD == password)
            {
                HttpContext.Session.setObject("USUARIO_LOGUEADO", usuario);

                return RedirectToAction("Index", "Statics");
            }
            else
            {
                ViewData["MENSAJE"] = "Email o contraseña incorrectos.";
                return View();
            }
        }
    }
}