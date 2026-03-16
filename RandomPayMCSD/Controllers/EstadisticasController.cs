using Microsoft.AspNetCore.Mvc;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;

namespace RandomPayMCSD.Controllers
{
    public class EstadisticasController : Controller
    {
        private readonly IRepositoryActividades _repoActividades;
        private readonly IRepositoryParticipantes _repoParticipantes;
        private readonly IRepositoryGastos _repoGastos;
        private readonly BalanceService _balanceService;

        public EstadisticasController(
            IRepositoryActividades repoActividades,
            IRepositoryParticipantes repoParticipantes,
            IRepositoryGastos repoGastos,
            BalanceService balanceService)
        {
            _repoActividades = repoActividades;
            _repoParticipantes = repoParticipantes;
            _repoGastos = repoGastos;
            _balanceService = balanceService;
        }

        public async Task<IActionResult> Index(int idActividad)
        {
            var actividad = await _repoActividades.GetByIdWithDetailsAsync(idActividad);
            if (actividad == null) return RedirectToAction("Index", "Statics");

            // 1. Datos para el gráfico: ¿Quién ha pagado más? (Gasto total por persona)
            var participantes = await _repoParticipantes.GetByActividadIdAsync(idActividad);
            var gastos = await _repoGastos.GetByActividadIdAsync(idActividad);

            var etiquetas = participantes.Select(p => p.NOMBREPARTICIPANTE).ToList();
            var totalesPagados = participantes.Select(p =>
                (double)gastos.Where(g => g.IDPAGADOR == p.IDPARTICIPANTE).Sum(g => g.IMPORTE)
            ).ToList();

            // 2. Datos de Balance Actual (lo que deben o les deben ahora mismo)
            var balances = await _balanceService.GetBalancesActividadAsync(idActividad);
            var nombresBalance = balances.Select(b => b.Participante).ToList();
            var montosBalance = balances.Select(b => b.Debe).ToList();

            ViewBag.Etiquetas = etiquetas;
            ViewBag.ValoresPagados = totalesPagados;
            ViewBag.NombresBalance = nombresBalance;
            ViewBag.MontosBalance = montosBalance;

            return View(actividad);
        }
    }
}