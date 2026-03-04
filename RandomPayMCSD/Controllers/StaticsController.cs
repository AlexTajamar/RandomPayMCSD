using Microsoft.AspNetCore.Mvc;
using RandomPayMCSD.Extensions;
using RandomPayMCSD.Interfaces;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;

namespace RandomPayMCSD.Controllers
{
    public class StaticsController : Controller
    {
        private IRepositoryActividades repoActividades;
        private IRepositoryDivisas repoDivisas;
        public StaticsController(IRepositoryActividades repoActividades,IRepositoryDivisas repoDivisas)
        {
            this.repoActividades = repoActividades;
            this.repoDivisas = repoDivisas;
        }

        public async Task<IActionResult> Index()
        {
            Usuario user = HttpContext.Session.getObject<Usuario>("USUARIO_LOGUEADO");

            if (user == null)
            {
                return RedirectToAction("Index", "RandomLogIn");
            }

            List<Actividad> misActividades = await this.repoActividades.GetByUsuarioIdAsync(user.IDUSUARIO);
            return View(misActividades);
        }

        public IActionResult RandomPay()
        {
            // Recuperamos la lista de la sesión. Si no existe, creamos una nueva vacía.
            List<string> participantes = HttpContext.Session.getObject<List<string>>("RULETA_NOMBRES") ?? new List<string>();

            // Si venimos de hacer el sorteo, leemos el ganador
            ViewBag.Ganador = TempData["GANADOR"]?.ToString();

            return View(participantes);
        }


        // --- 2. AÑADIR UN NOMBRE ---
        [HttpPost]
        public IActionResult AddNombreRandom(string nombre)
        {
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                List<string> participantes = HttpContext.Session.getObject<List<string>>("RULETA_NOMBRES") ?? new List<string>();

                // Evitamos duplicados
                if (!participantes.Contains(nombre.Trim()))
                {
                    participantes.Add(nombre.Trim());
                    HttpContext.Session.setObject("RULETA_NOMBRES", participantes);
                }
            }
            return RedirectToAction("RandomPay");
        }

        // --- 3. ELIMINAR UN NOMBRE ---
        [HttpPost]
        public IActionResult RemoveNombreRandom(string nombre)
        {
            List<string> participantes = HttpContext.Session.getObject<List<string>>("RULETA_NOMBRES") ?? new List<string>();
            participantes.Remove(nombre);
            HttpContext.Session.setObject("RULETA_NOMBRES", participantes);

            return RedirectToAction("RandomPay");
        }

        // --- 4. HACER EL SORTEO EN C# ---
        [HttpPost]
        public IActionResult SortearRandom()
        {
            List<string> participantes = HttpContext.Session.getObject<List<string>>("RULETA_NOMBRES") ?? new List<string>();

            if (participantes.Count > 1)
            {
                // La lógica matemática del azar en el Backend
                Random rnd = new Random();
                int index = rnd.Next(participantes.Count);
                string perdedor = participantes[index];

                // Guardamos al elegido en un TempData para mostrarlo en la vista al recargar
                TempData["GANADOR"] = perdedor;
            }

            return RedirectToAction("RandomPay");
        }

        // --- 5. LIMPIAR LA RULETA ---
        [HttpPost]
        public IActionResult ResetRandom()
        {
            HttpContext.Session.Remove("RULETA_NOMBRES");
            return RedirectToAction("RandomPay");
        }
        public async Task<IActionResult> Divisas()
        {
            var divisas = await this.repoDivisas.GetDivisasAsync();
            ViewBag.Resultado = 0;
            return View(divisas);
        }
        [HttpPost]
        public async Task<IActionResult> Divisas(double? importe, string origen, string destino)
        {
            // Cargamos siempre las divisas para los selects de la vista
            var divisas = await this.repoDivisas.GetDivisasAsync();

            if (importe != null && importe > 0)
            {
                var dOrigen = await this.repoDivisas.GetDivisaByCodigoAsync(origen);
                var dDestino = await this.repoDivisas.GetDivisaByCodigoAsync(destino);

                if (dOrigen != null && dDestino != null)
                {
                    // Fórmula: (Importe / Tasa de Origen) * Tasa de Destino
                    double resultado = (importe.Value / dOrigen.Tasa) * dDestino.Tasa;

                    ViewBag.Importe = importe;
                    ViewBag.Origen = origen;
                    ViewBag.Destino = destino;
                    ViewBag.Resultado = Math.Round(resultado, 2);
                }
            }

            return View(divisas);
        }
    }
}