using Microsoft.AspNetCore.Mvc;
using RandomPayMCSD.Extensions;
using RandomPayMCSD.Interfaces;
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
        private readonly IRepositoryRepartos _repoRepartos;
        private readonly IRepositoryDivisas _repoDivisas;
        private readonly BalanceService _balanceService;
        private readonly InvitationService _invitationService;

        public ActividadesController(
            IRepositoryActividades repoActividades,
            IRepositoryGastos repoGastos,
            IRepositoryDivisas repoDivisas,
            IRepositoryParticipantes repoParticipantes,
            IRepositoryRepartos repoRepartos,
            BalanceService balanceService,
            InvitationService invitationService)
        {
            _repoActividades = repoActividades;
            _repoGastos = repoGastos;
            _repoParticipantes = repoParticipantes;
            _balanceService = balanceService;
            _invitationService = invitationService;
            _repoRepartos = repoRepartos;
            _repoDivisas = repoDivisas;
        }

        // --- 1. DETALLE DE LA ACTIVIDAD ---
        public async Task<IActionResult> Detalle(int id)
        {
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(id);
            if (actividad == null) return RedirectToAction("Index", "Statics");

            // Calculamos quién debe a quién
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

        // --- 3. MOSTRAR EL FORMULARIO DE GASTO CON REPARTO (GET) ---
        public async Task<IActionResult> AddGasto(int id)
        {
            ViewBag.Participantes = await _repoParticipantes.GetByActividadIdAsync(id);
            ViewBag.Divisas = await _repoDivisas.GetDivisasAsync();

            Gasto modeloGasto = new Gasto { IDACTIVIDAD = id, FECHA = DateTime.Now };

            return View(modeloGasto);
        }

        [HttpPost]
        public async Task<IActionResult> AddGasto(
     Gasto gastoNormal,
     string importeString, // <--- AÑADIDO
     string divisaSeleccionada,
     List<int> idsParticipantes,
     List<string> cantidadesAsignadas)
        {
            // --- MAGIA PARA ARREGLAR LOS DECIMALES DEL TOTAL ---
            if (!string.IsNullOrWhiteSpace(importeString))
            {
                string totalLimpio = importeString.Replace(",", ".");
                if (double.TryParse(totalLimpio, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double totalParseado))
                {
                    gastoNormal.IMPORTE = (decimal)totalParseado;
                }
            }

            if (gastoNormal.IMPORTE <= 0)
            {
                ModelState.AddModelError("IMPORTE", "El importe debe ser mayor que cero.");
                ViewBag.Participantes = await _repoParticipantes.GetByActividadIdAsync(gastoNormal.IDACTIVIDAD);
                ViewBag.Divisas = await _repoDivisas.GetDivisasAsync();
                return View(gastoNormal);
            }

            if (divisaSeleccionada != "EUR")
            {
                var monedaOrigen = await _repoDivisas.GetDivisaByCodigoAsync(divisaSeleccionada);
                if (monedaOrigen != null)
                {
                    gastoNormal.IMPORTE = Math.Round(gastoNormal.IMPORTE / (decimal)monedaOrigen.Tasa, 2);
                }
            }

            gastoNormal.FECHA = DateTime.Now;
            await _repoGastos.AddAsync(gastoNormal);

            if (idsParticipantes != null && cantidadesAsignadas != null)
            {
                for (int i = 0; i < idsParticipantes.Count; i++)
                {
                    double cuantoDebe = 0;
                    if (i < cantidadesAsignadas.Count && !string.IsNullOrWhiteSpace(cantidadesAsignadas[i]))
                    {
                        string valorLimpio = cantidadesAsignadas[i].Replace(",", ".");
                        double.TryParse(valorLimpio, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out cuantoDebe);
                    }

                    if (cuantoDebe > 0)
                    {
                        if (divisaSeleccionada != "EUR")
                        {
                            var moneda = await _repoDivisas.GetDivisaByCodigoAsync(divisaSeleccionada);
                            cuantoDebe = Math.Round(cuantoDebe / moneda.Tasa, 2);
                        }

                        RepartoGasto reparto = new RepartoGasto
                        {
                            IdGasto = gastoNormal.IDGASTO,
                            IdParticipante = idsParticipantes[i],
                            Cantidad = cuantoDebe
                        };
                        await _repoRepartos.AddAsync(reparto);
                    }
                }
            }

            return RedirectToAction("Detalle", new { id = gastoNormal.IDACTIVIDAD });
        }
        // --- 5. LA RULETA RANDOM ---
        public async Task<IActionResult> Ruleta(int id)
        {
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(id);
            if (actividad == null) return RedirectToAction("Index", "Statics");

            return View(actividad);
        }

        [HttpPost]
        public async Task<IActionResult> TirarRuleta(int idActividad)
        {
            Participante perdedor = await _balanceService.ElegirPagadorAlAzar(idActividad);
            return Json(new { nombre = perdedor.NOMBREPARTICIPANTE, id = perdedor.IDPARTICIPANTE });
        }

        // --- MOSTRAR EL FORMULARIO DE EDICIÓN (GET) ---
        public async Task<IActionResult> EditGasto(int idGasto)
        {
            // 1. Usamos tu método exacto GetByIdAsync
            Gasto gasto = await _repoGastos.GetByIdAsync(idGasto);

            if (gasto == null) return RedirectToAction("Index", "Statics");

            // 2. Cargamos las listas necesarias
            ViewBag.Participantes = await _repoParticipantes.GetByActividadIdAsync(gasto.IDACTIVIDAD);
            ViewBag.Divisas = await _repoDivisas.GetDivisasAsync();

            // 3. Cargamos los repartos que ya estaban guardados
            ViewBag.Repartos = await _repoRepartos.GetRepartosByGastoAsync(idGasto);

            return View("AddGasto", gasto); // Usamos la misma vista que para añadir
        }
        [HttpPost]
        public async Task<IActionResult> EditGasto(
     Gasto gastoEditado,
     string importeString, // <--- AÑADIDO
     string divisaSeleccionada,
     List<int> idsParticipantes,
     List<string> cantidadesAsignadas)
        {
            // --- MAGIA PARA ARREGLAR LOS DECIMALES DEL TOTAL ---
            if (!string.IsNullOrWhiteSpace(importeString))
            {
                string totalLimpio = importeString.Replace(",", ".");
                if (double.TryParse(totalLimpio, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double totalParseado))
                {
                    gastoEditado.IMPORTE = (decimal)totalParseado;
                }
            }

            if (divisaSeleccionada != "EUR")
            {
                var monedaOrigen = await _repoDivisas.GetDivisaByCodigoAsync(divisaSeleccionada);
                if (monedaOrigen != null)
                {
                    gastoEditado.IMPORTE = Math.Round(gastoEditado.IMPORTE / (decimal)monedaOrigen.Tasa, 2);
                }
            }

            if (gastoEditado.FECHA.Year == 1)
            {
                gastoEditado.FECHA = DateTime.Now;
            }

            await _repoGastos.UpdateAsync(gastoEditado);

            var repartosAntiguos = await _repoRepartos.GetRepartosByGastoAsync(gastoEditado.IDGASTO);
            foreach (var rep in repartosAntiguos)
            {
                await _repoRepartos.DeleteAsync(rep.IdReparto);
            }

            if (idsParticipantes != null && cantidadesAsignadas != null)
            {
                for (int i = 0; i < idsParticipantes.Count; i++)
                {
                    double cuantoDebe = 0;
                    if (i < cantidadesAsignadas.Count && !string.IsNullOrWhiteSpace(cantidadesAsignadas[i]))
                    {
                        string valorLimpio = cantidadesAsignadas[i].Replace(",", ".");
                        double.TryParse(valorLimpio, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out cuantoDebe);
                    }

                    if (cuantoDebe > 0)
                    {
                        if (divisaSeleccionada != "EUR")
                        {
                            var moneda = await _repoDivisas.GetDivisaByCodigoAsync(divisaSeleccionada);
                            cuantoDebe = Math.Round(cuantoDebe / moneda.Tasa, 2);
                        }

                        await _repoRepartos.AddAsync(new RepartoGasto
                        {
                            IdGasto = gastoEditado.IDGASTO,
                            IdParticipante = idsParticipantes[i],
                            Cantidad = cuantoDebe
                        });
                    }
                }
            }

            return RedirectToAction("Detalle", new { id = gastoEditado.IDACTIVIDAD });
        }

        // --- BORRAR GASTO (POST) ---
        [HttpPost]
        public async Task<IActionResult> DeleteGasto(int idGasto)
        {
            // 1. Buscamos el gasto para saber a qué actividad pertenece y poder volver a ella
            Gasto gasto = await _repoGastos.GetByIdAsync(idGasto);

            if (gasto != null)
            {
                // 2. PRIMERO borramos los repartos asociados a este gasto (para evitar error de Foreign Key)
                var repartosAsociados = await _repoRepartos.GetRepartosByGastoAsync(idGasto);
                foreach (var rep in repartosAsociados)
                {
                    await _repoRepartos.DeleteAsync(rep.IdReparto);
                }

                // 3. SEGUNDO borramos el gasto de la tabla principal
                await _repoGastos.DeleteAsync(idGasto);

                return RedirectToAction("Detalle", new { id = gasto.IDACTIVIDAD });
            }

            return RedirectToAction("Index", "Statics");
        }
    }
}