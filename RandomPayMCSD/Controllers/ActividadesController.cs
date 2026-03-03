using Microsoft.AspNetCore.Mvc;
using RandomPayMCSD.Extensions;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;

namespace RandomPayMCSD.Controllers
{
    public class ActividadesController : Controller
    {
        private readonly IRepositoryActividades _repoActividades;
        private readonly IRepositoryGastos _repoGastos;
        private readonly IRepositoryParticipantes _repoParticipantes;
        private readonly BalanceService _balanceService;
        private readonly InvitationService _invitationService;

        public ActividadesController(
            IRepositoryActividades repoActividades,
            IRepositoryGastos repoGastos,
            IRepositoryParticipantes repoParticipantes,
            BalanceService balanceService,
            InvitationService invitationService)
        {
            _repoActividades = repoActividades;
            _repoGastos = repoGastos;
            _repoParticipantes = repoParticipantes;
            _balanceService = balanceService;
            _invitationService = invitationService;
        }

        // --- 1. DETALLE DE LA ACTIVIDAD ---
        public async Task<IActionResult> Detalle(int id)
        {
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(id);
            if (actividad == null) return RedirectToAction("Index", "Statics");

            // Calculamos quién debe a quién usando tu servicio
            ViewBag.Balances = await _balanceService.GetBalancesActividadAsync(id);

            return View(actividad);
        }

        // --- 2. CREAR ACTIVIDAD ---
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(string nombre, string moneda)
        {
            Usuario user = HttpContext.Session.getObject<Usuario>("USUARIO_LOGUEADO");
            if (user == null) return RedirectToAction("Index", "RandomLogIn");

            // Creamos la actividad
            Actividad nuevaActividad = new Actividad
            {
                NOMBREACTIVIDAD = nombre,
                MONEDAPRINCIPAL = moneda ?? "EUR",
                IDCREADOR = user.IDUSUARIO,
                FECHACREACION = DateTime.Now,
                INVITACIONCOD = _invitationService.GenerarCodigoUnico()
            };

            await _repoActividades.AddAsync(nuevaActividad);

            // MUY IMPORTANTE: El creador se añade automáticamente como participante
            Participante creadorParticipante = new Participante
            {
                IDACTIVIDAD = nuevaActividad.IDACTIVIDAD,
                NOMBREPARTICIPANTE = user.NOMBRE,
                IDUSUARIO = user.IDUSUARIO
            };
            await _repoParticipantes.AddAsync(creadorParticipante);

            return RedirectToAction("Detalle", new { id = nuevaActividad.IDACTIVIDAD });
        }

        // --- 3. AÑADIR GASTO ---
        public async Task<IActionResult> AddGasto(int id)
        {
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(id);
            if (actividad == null) return RedirectToAction("Index", "Statics");

            ViewBag.Participantes = actividad.Participantes.ToList();
            ViewBag.Moneda = actividad.MONEDAPRINCIPAL;

            Gasto nuevoGasto = new Gasto { IDACTIVIDAD = id, FECHA = DateTime.Now };
            return View(nuevoGasto);
        }

        [HttpPost]
        public async Task<IActionResult> AddGasto(Gasto gasto)
        {
            if (gasto.IMPORTE <= 0)
            {
                ModelState.AddModelError("IMPORTE", "El importe debe ser mayor que cero.");
            }

            if (ModelState.IsValid)
            {
                await _repoGastos.AddAsync(gasto);
                return RedirectToAction("Detalle", new { id = gasto.IDACTIVIDAD });
            }

            // Si hay error, recargamos la vista
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(gasto.IDACTIVIDAD);
            ViewBag.Participantes = actividad.Participantes.ToList();
            ViewBag.Moneda = actividad.MONEDAPRINCIPAL;

            return View(gasto);
        }

        // --- 4. LA RULETA RANDOM ---
        public async Task<IActionResult> Ruleta(int id)
        {
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(id);
            if (actividad == null) return RedirectToAction("Index", "Statics");

            // Pasamos la actividad a la vista para tener los datos
            return View(actividad);
        }

        [HttpPost]
        public async Task<IActionResult> TirarRuleta(int idActividad)
        {
            Participante perdedor = await _balanceService.ElegirPagadorAlAzar(idActividad);

            // Devolvemos el nombre en formato JSON para que la vista pueda hacer una animación chula
            return Json(new { nombre = perdedor.NOMBREPARTICIPANTE, id = perdedor.IDPARTICIPANTE });
        }

       
    }
}