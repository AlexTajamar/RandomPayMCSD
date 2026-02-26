using Microsoft.AspNetCore.Mvc;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories;
using RandomPayMCSD.Extensions;

namespace RandomPayMCSD.Controllers
{
    public class StaticsController : Controller
    {
        private RepositoryActividades repoActividades;

        public StaticsController(RepositoryActividades repoActividades)
        {
            this.repoActividades = repoActividades;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Recuperamos el usuario de la sesión
            Usuario user = HttpContext.Session.getObject<Usuario>("USUARIO_LOGUEADO");

            // 2. Si no hay usuario, devolvemos la vista vacía (la vista ya mostrará el error "No has iniciado sesión")
            if (user == null)
            {
                return View();
            }

            // 3. Si hay usuario, buscamos sus actividades usando su ID
            List<Actividad> misActividades = await this.repoActividades.GetActividadesUsuarioAsync(user.IDUSUARIO);

            // 4. Pasamos la lista a la vista
            return View(misActividades);
        }
    }
}