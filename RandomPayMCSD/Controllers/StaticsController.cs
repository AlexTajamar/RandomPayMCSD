using Microsoft.AspNetCore.Mvc;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Extensions;

namespace RandomPayMCSD.Controllers
{
    public class StaticsController : Controller
    {
        private IRepositoryActividades repoActividades;

        public StaticsController(IRepositoryActividades repoActividades)
        {
            this.repoActividades = repoActividades;
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

        // --- 1. MOSTRAR LA VISTA DE DIVISAS ---
 
        // --- 2. LOGICA DE CONVERSIÓN (C#) ---
        // --- 1. LISTADO DE TASAS (Base 1 EUR) ---
        private Dictionary<string, double> GetTasas()
        {
            return new Dictionary<string, double>
    {
        { "EUR", 1.0 },    { "USD", 1.09 },   { "GBP", 0.86 },   { "JPY", 163.5 },
        { "CHF", 0.95 },   { "CAD", 1.48 },   { "AUD", 1.66 },   { "CNY", 7.85 },
        { "HKD", 8.52 },   { "NZD", 1.80 },   { "SEK", 11.35 },  { "NOK", 11.45 },
        { "DKK", 7.46 },   { "INR", 90.50 },  { "BRL", 5.45 },   { "ZAR", 20.60 },
        { "MXN", 18.50 },  { "SGD", 1.46 },   { "KRW", 1450.0 }, { "TRY", 34.00 },
        { "PLN", 4.32 }
    };
        }

        // --- 2. VISTA (GET) ---
        public IActionResult Divisas()
        {
            ViewBag.Tasas = GetTasas();
            ViewBag.Resultado = 0;
            return View();
        }

        // --- 3. CÁLCULO (POST) ---
        [HttpPost]
        public IActionResult Divisas(double importe, string origen, string destino)
        {
            var tasas = GetTasas();
            ViewBag.Tasas = tasas; // Para que el desplegable no se vacíe al recargar

            if (importe > 0 && tasas.ContainsKey(origen) && tasas.ContainsKey(destino))
            {
                // Lógica de conversión sencilla
                double importeEnEuros = importe / tasas[origen];
                double resultado = importeEnEuros * tasas[destino];

                ViewBag.Importe = importe;
                ViewBag.Origen = origen;
                ViewBag.Destino = destino;
                ViewBag.Resultado = Math.Round(resultado, 2);
            }

            return View();
        }
    }
}