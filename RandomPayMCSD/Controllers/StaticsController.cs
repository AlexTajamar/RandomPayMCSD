using Microsoft.AspNetCore.Authorization; // <--- AÑADIDO
using Microsoft.AspNetCore.Mvc;
using RandomPayMCSD.Extensions;
using RandomPayMCSD.Interfaces;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using System.Globalization;
using System.Security.Claims; // <--- AÑADIDO

namespace RandomPayMCSD.Controllers
{
    [Authorize] // <--- BLINDAJE APLICADO
    public class StaticsController : Controller
    {
        private IRepositoryActividades repoActividades;
        private IRepositoryDivisas repoDivisas;

        public StaticsController(IRepositoryActividades repoActividades, IRepositoryDivisas repoDivisas)
        {
            this.repoActividades = repoActividades;
            this.repoDivisas = repoDivisas;
        }

        public async Task<IActionResult> Index()
        {
            // LEEMOS DESDE LOS CLAIMS (Ya no necesitamos comprobar si es null, porque [Authorize] nos garantiza que está logueado)
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Actividad> misActividades = await this.repoActividades.GetByUsuarioIdAsync(idUsuario);
            return View(misActividades);
        }

        public IActionResult RandomPay()
        {
            List<string> participantes = HttpContext.Session.getObject<List<string>>("RULETA_NOMBRES") ?? new List<string>();
            ViewBag.Ganador = TempData["GANADOR"]?.ToString();
            return View(participantes);
        }

        [HttpPost]
        public IActionResult AddNombreRandom(string nombre)
        {
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                List<string> participantes = HttpContext.Session.getObject<List<string>>("RULETA_NOMBRES") ?? new List<string>();
                if (!participantes.Contains(nombre.Trim()))
                {
                    participantes.Add(nombre.Trim());
                    HttpContext.Session.setObject("RULETA_NOMBRES", participantes);
                }
            }
            return RedirectToAction("RandomPay");
        }

        [HttpPost]
        public IActionResult RemoveNombreRandom(string nombre)
        {
            List<string> participantes = HttpContext.Session.getObject<List<string>>("RULETA_NOMBRES") ?? new List<string>();
            participantes.Remove(nombre);
            HttpContext.Session.setObject("RULETA_NOMBRES", participantes);
            return RedirectToAction("RandomPay");
        }

        [HttpPost]
        public IActionResult SortearRandom()
        {
            List<string> participantes = HttpContext.Session.getObject<List<string>>("RULETA_NOMBRES") ?? new List<string>();

            if (participantes.Count > 1)
            {
                Random rnd = new Random();
                int index = rnd.Next(participantes.Count);
                string perdedor = participantes[index];

                TempData["GANADOR"] = perdedor;
            }

            return RedirectToAction("RandomPay");
        }

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
        public async Task<IActionResult> Divisas(string importeString, string origen, string destino)
        {
            var divisas = await this.repoDivisas.GetDivisasAsync();

            if (!string.IsNullOrWhiteSpace(importeString))
            {
                if (double.TryParse(importeString.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double importeParseado) && importeParseado > 0)
                {
                    var dOrigen = await this.repoDivisas.GetDivisaByCodigoAsync(origen);
                    var dDestino = await this.repoDivisas.GetDivisaByCodigoAsync(destino);

                    if (dOrigen != null && dDestino != null)
                    {
                        double resultado = (importeParseado / dOrigen.Tasa) * dDestino.Tasa;

                        ViewBag.Importe = importeParseado;
                        ViewBag.Origen = origen;
                        ViewBag.Destino = destino;
                        ViewBag.Resultado = Math.Round(resultado, 2);
                    }
                }
            }
            return View(divisas);
        }
    }
}