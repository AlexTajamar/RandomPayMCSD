using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RandomPayMCSD.Extensions;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces; // Aseguramos usar la carpeta de interfaces
using System.Threading.Tasks;

namespace RandomPayMCSD.Controllers
{
    public class RandomLogInController : Controller
    {
        // 1. Aquí pedimos la INTERFAZ, no la clase
        private IRepositoryUsuarios repo;

        public RandomLogInController(IRepositoryUsuarios repo)
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
            // 2. Aquí usamos el nombre correcto del método
            Usuario usuario = await this.repo.GetByEmailAsync(email);
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
        public IActionResult LogOut()
        {
            // Destruimos la sesión del usuario
            HttpContext.Session.Remove("USUARIO_LOGUEADO");

            // Lo mandamos de vuelta a la pantalla de Login
            return RedirectToAction("Index", "RandomLogIn");
        }
    }
}