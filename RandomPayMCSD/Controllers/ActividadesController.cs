using Microsoft.AspNetCore.Mvc;
using RandomPayMCSD.Extensions;
using RandomPayMCSD.Interfaces;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;
using System.Globalization;

namespace RandomPayMCSD.Controllers
{
    public class ActividadesController : Controller
    {
        private readonly IRepositoryActividades _repoActividades;
        private readonly IRepositoryGastos _repoGastos;
        private readonly IRepositoryParticipantes _repoParticipantes;
        private readonly IRepositoryRepartos _repoRepartos;
        private readonly IRepositoryDivisas _repoDivisas;
        private readonly IRepositoryListaCompra _repoListaCompra; // <--- AÑADIDO
        private readonly BalanceService _balanceService;
        private readonly InvitationService _invitationService;

        public ActividadesController(
            IRepositoryActividades repoActividades,
            IRepositoryListaCompra repoListaCompra, // <--- AÑADIDO
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
            _repoListaCompra = repoListaCompra; // <--- AÑADIDO
        }

        // --- 1. DETALLE Y GESTIÓN DE ACTIVIDAD ---

        public async Task<IActionResult> Detalle(int id)
        {
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(id);
            if (actividad == null) return RedirectToAction("Index", "Statics");

            ViewBag.Balances = await _balanceService.GetBalancesActividadAsync(id);
            ViewBag.Transferencias = await _balanceService.GetTransferenciasAsync(id);

            // ---> ¡FALTABA ESTO! Cargar la lista de la compra de la base de datos
            ViewBag.ListaCompra = await _repoListaCompra.GetByActividadAsync(id);

            return View(actividad);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(string nombre, string moneda, List<string> nombresAmigos, IFormFile imagenForm, string emojiForm)
        {
            Usuario user = HttpContext.Session.getObject<Usuario>("USUARIO_LOGUEADO");
            if (user == null) return RedirectToAction("Index", "RandomLogIn");

            string imagenFinal = "✈️";

            if (imagenForm != null && imagenForm.Length > 0)
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "actividades");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagenForm.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagenForm.CopyToAsync(stream);
                }
                imagenFinal = "/images/actividades/" + fileName;
            }
            else if (!string.IsNullOrWhiteSpace(emojiForm))
            {
                imagenFinal = emojiForm.Trim();
            }

            Actividad nuevaActividad = new Actividad
            {
                NOMBREACTIVIDAD = nombre,
                MONEDAPRINCIPAL = moneda ?? "EUR",
                IDCREADOR = user.IDUSUARIO,
                FECHACREACION = DateTime.Now,
                INVITACIONCOD = _invitationService.GenerarCodigoUnico(),
                IMAGEN = imagenFinal
            };
            await _repoActividades.AddAsync(nuevaActividad);

            Participante creadorParticipante = new Participante
            {
                IDACTIVIDAD = nuevaActividad.IDACTIVIDAD,
                NOMBREPARTICIPANTE = user.NOMBRE,
                IDUSUARIO = user.IDUSUARIO
            };
            await _repoParticipantes.AddAsync(creadorParticipante);

            if (nombresAmigos != null)
            {
                foreach (var nombreAmigo in nombresAmigos.Where(n => !string.IsNullOrWhiteSpace(n)))
                {
                    await _repoParticipantes.AddAsync(new Participante
                    {
                        IDACTIVIDAD = nuevaActividad.IDACTIVIDAD,
                        NOMBREPARTICIPANTE = nombreAmigo.Trim(),
                        IDUSUARIO = null
                    });
                }
            }

            return RedirectToAction("Detalle", new { id = nuevaActividad.IDACTIVIDAD });
        }

        public async Task<IActionResult> AddParticipante(int id)
        {
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(id);
            if (actividad == null) return RedirectToAction("Index", "Statics");
            return View(actividad);
        }

        [HttpPost]
        public async Task<IActionResult> AddParticipante(int IDACTIVIDAD, string nombreParticipante)
        {
            if (!string.IsNullOrWhiteSpace(nombreParticipante))
            {
                await _repoParticipantes.AddAsync(new Participante
                {
                    IDACTIVIDAD = IDACTIVIDAD,
                    NOMBREPARTICIPANTE = nombreParticipante,
                    IDUSUARIO = null
                });
            }
            return RedirectToAction("Detalle", new { id = IDACTIVIDAD });
        }

        // --- 2. GESTIÓN DE INVITACIONES Y VINCULACIÓN ---

        [HttpGet]
        public IActionResult Unirse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Unirse(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return View();

            Usuario user = HttpContext.Session.getObject<Usuario>("USUARIO_LOGUEADO");
            var actividad = await _repoActividades.GetByCodigoAsync(codigo);

            if (actividad == null)
            {
                ViewBag.Error = "No existe ninguna actividad con el código: " + codigo.ToUpper();
                return View();
            }

            if (actividad.Participantes.Any(p => p.IDUSUARIO == user.IDUSUARIO))
            {
                ViewBag.Error = "Ya estás dentro de esta actividad. Búscala en tu panel de inicio.";
                return View();
            }

            return RedirectToAction("Vincular", new { id = actividad.IDACTIVIDAD });
        }

        [HttpGet]
        public async Task<IActionResult> Vincular(int id)
        {
            Usuario user = HttpContext.Session.getObject<Usuario>("USUARIO_LOGUEADO");
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(id);

            if (actividad == null) return RedirectToAction("Index", "Statics");

            if (actividad.Participantes.Any(p => p.IDUSUARIO == user.IDUSUARIO))
            {
                return RedirectToAction("Detalle", new { id = id });
            }

            ViewBag.Huerfanos = actividad.Participantes.Where(p => p.IDUSUARIO == null).ToList();
            return View(actividad);
        }

        [HttpPost]
        public async Task<IActionResult> Vincular(int idActividad, int idParticipante)
        {
            Usuario user = HttpContext.Session.getObject<Usuario>("USUARIO_LOGUEADO");
            Participante p = await _repoParticipantes.GetByIdAsync(idParticipante);

            if (p != null)
            {
                p.IDUSUARIO = user.IDUSUARIO;
                await _repoParticipantes.UpdateAsync(p);
            }

            return RedirectToAction("Detalle", new { id = idActividad });
        }

        [HttpPost]
        public async Task<IActionResult> VincularNuevo(int idActividad)
        {
            Usuario user = HttpContext.Session.getObject<Usuario>("USUARIO_LOGUEADO");
            if (user == null) return RedirectToAction("Index", "RandomLogIn");

            await _repoParticipantes.AddAsync(new Participante
            {
                IDACTIVIDAD = idActividad,
                NOMBREPARTICIPANTE = user.NOMBRE,
                IDUSUARIO = user.IDUSUARIO
            });

            return RedirectToAction("Detalle", new { id = idActividad });
        }

        // --- 3. NUEVOS MÉTODOS: LISTA DE LA COMPRA ---

        [HttpPost]
        public async Task<IActionResult> AddItemCompra(int idActividad, string nombreItem, string precioEstimadoStr)
        {
            // AÑADIDO: Control de valores negativos (estimado >= 0)
            if (double.TryParse(precioEstimadoStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double estimado) && estimado >= 0)
            {
                await _repoListaCompra.AddAsync(new ItemCompra
                {
                    IdActividad = idActividad,
                    NombreItem = nombreItem,
                    PrecioEstimado = (decimal)Math.Round(estimado, 2)
                });
            }
            return RedirectToAction("Detalle", new { id = idActividad });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteItemCompra(int idItem, int idActividad)
        {
            await _repoListaCompra.DeleteAsync(idItem);
            return RedirectToAction("Detalle", new { id = idActividad });
        }

        // --- 4. GASTOS Y DEUDAS ---

        public async Task<IActionResult> AddGasto(int id, int? idItemCompra)
        {
            ViewBag.Participantes = await _repoParticipantes.GetByActividadIdAsync(id);
            ViewBag.Divisas = await _repoDivisas.GetDivisasAsync();

            Gasto gasto = new Gasto { IDACTIVIDAD = id, FECHA = DateTime.Now };

            if (idItemCompra.HasValue)
            {
                var item = await _repoListaCompra.GetByIdAsync(idItemCompra.Value);
                if (item != null && !item.Comprado)
                {
                    gasto.CONCEPTO = item.NombreItem;
                    gasto.IMPORTE = item.PrecioEstimado;
                    ViewBag.IdItemCompra = item.IdItem;
                }
            }

            return View(gasto);
        }

        [HttpPost]
        public async Task<IActionResult> AddGasto(Gasto gastoNormal, string importeString, string divisaSeleccionada, List<int> idsParticipantes, List<string> cantidadesAsignadas, int? idItemCompra)
        {
            if (!string.IsNullOrWhiteSpace(importeString))
            {
                if (double.TryParse(importeString.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double totalParseado))
                {
                    gastoNormal.IMPORTE = Math.Round((decimal)totalParseado, 2);
                }
            }

            if (gastoNormal.IMPORTE <= 0)
            {
                ModelState.AddModelError("IMPORTE", "Importe no válido.");
                ViewBag.Participantes = await _repoParticipantes.GetByActividadIdAsync(gastoNormal.IDACTIVIDAD);
                ViewBag.Divisas = await _repoDivisas.GetDivisasAsync();
                return View(gastoNormal);
            }

            if (divisaSeleccionada != "EUR")
            {
                var moneda = await _repoDivisas.GetDivisaByCodigoAsync(divisaSeleccionada);
                if (moneda != null) gastoNormal.IMPORTE = Math.Round(gastoNormal.IMPORTE / (decimal)moneda.Tasa, 2);
            }

            gastoNormal.FECHA = DateTime.Now;
            await _repoGastos.AddAsync(gastoNormal);

            if (idsParticipantes != null)
            {
                for (int i = 0; i < idsParticipantes.Count; i++)
                {
                    if (i < cantidadesAsignadas.Count && double.TryParse(cantidadesAsignadas[i].Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double valor) && valor > 0)
                    {
                        double valorRedondeado = Math.Round(valor, 2);
                        if (divisaSeleccionada != "EUR")
                        {
                            var moneda = await _repoDivisas.GetDivisaByCodigoAsync(divisaSeleccionada);
                            valorRedondeado = Math.Round(valorRedondeado / moneda.Tasa, 2);
                        }
                        await _repoRepartos.AddAsync(new RepartoGasto { IdGasto = gastoNormal.IDGASTO, IdParticipante = idsParticipantes[i], Cantidad = valorRedondeado });
                    }
                }
            }

            if (idItemCompra.HasValue)
            {
                var item = await _repoListaCompra.GetByIdAsync(idItemCompra.Value);
                if (item != null)
                {
                    item.Comprado = true;
                    item.IdGasto = gastoNormal.IDGASTO;
                    await _repoListaCompra.UpdateAsync(item);
                }
            }

            return RedirectToAction("Detalle", new { id = gastoNormal.IDACTIVIDAD });
        }

        // --- MÉTODOS DE EDICIÓN ---

        [HttpGet]
        public async Task<IActionResult> EditGasto(int idGasto)
        {
            Gasto gasto = await _repoGastos.GetByIdAsync(idGasto);
            if (gasto == null) return RedirectToAction("Index", "Statics");

            ViewBag.Participantes = await _repoParticipantes.GetByActividadIdAsync(gasto.IDACTIVIDAD);
            ViewBag.Divisas = await _repoDivisas.GetDivisasAsync();
            ViewBag.Repartos = await _repoRepartos.GetRepartosByGastoAsync(idGasto);

            return View("AddGasto", gasto);
        }

        [HttpPost]
        public async Task<IActionResult> EditGasto(Gasto gastoEditado, string importeString, string divisaSeleccionada, List<int> idsParticipantes, List<string> cantidadesAsignadas)
        {
            if (!string.IsNullOrWhiteSpace(importeString))
            {
                if (double.TryParse(importeString.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double total))
                {
                    gastoEditado.IMPORTE = Math.Round((decimal)total, 2);
                }
            }

            await _repoGastos.UpdateAsync(gastoEditado);

            var antiguos = await _repoRepartos.GetRepartosByGastoAsync(gastoEditado.IDGASTO);
            foreach (var r in antiguos) await _repoRepartos.DeleteAsync(r.IdReparto);

            if (idsParticipantes != null)
            {
                for (int i = 0; i < idsParticipantes.Count; i++)
                {
                    if (i < cantidadesAsignadas.Count && double.TryParse(cantidadesAsignadas[i].Replace(",", "."), out double valor))
                    {
                        double valorRedondeado = Math.Round(valor, 2);
                        if (valorRedondeado > 0)
                        {
                            await _repoRepartos.AddAsync(new RepartoGasto
                            {
                                IdGasto = gastoEditado.IDGASTO,
                                IdParticipante = idsParticipantes[i],
                                Cantidad = valorRedondeado
                            });
                        }
                    }
                }
            }
            return RedirectToAction("Detalle", new { id = gastoEditado.IDACTIVIDAD });
        }

        [HttpPost]
        public async Task<IActionResult> SaldarDeuda(int idActividad, int idDeudor, int idAcreedor, string cantidadString)
        {
            if (double.TryParse(cantidadString.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double cantidad) && cantidad > 0)
            {
                Gasto reembolso = new Gasto { IDACTIVIDAD = idActividad, IDPAGADOR = idDeudor, CONCEPTO = "💸 Reembolso de deuda", IMPORTE = (decimal)Math.Round(cantidad, 2), FECHA = DateTime.Now };
                await _repoGastos.AddAsync(reembolso);
                await _repoRepartos.AddAsync(new RepartoGasto { IdGasto = reembolso.IDGASTO, IdParticipante = idAcreedor, Cantidad = Math.Round(cantidad, 2) });
            }
            return RedirectToAction("Detalle", new { id = idActividad });
        }

        // --- 5. OTROS MÉTODOS (Ruleta, Delete, etc.) ---

        public async Task<IActionResult> Ruleta(int id)
        {
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(id);
            return View(actividad);
        }

        [HttpPost]
        public async Task<IActionResult> TirarRuleta(int idActividad)
        {
            Participante perdedor = await _balanceService.ElegirPagadorAlAzar(idActividad);
            return Json(new { nombre = perdedor.NOMBREPARTICIPANTE, id = perdedor.IDPARTICIPANTE });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteActividad(int idActividad)
        {
            var itemsCompra = await _repoListaCompra.GetByActividadAsync(idActividad);
            foreach (var item in itemsCompra)
            {
                await _repoListaCompra.DeleteAsync(item.IdItem);
            }

            var gastos = await _repoGastos.GetByActividadIdAsync(idActividad);
            foreach (var gasto in gastos)
            {
                var repartos = await _repoRepartos.GetRepartosByGastoAsync(gasto.IDGASTO);
                foreach (var rep in repartos) await _repoRepartos.DeleteAsync(rep.IdReparto);
                await _repoGastos.DeleteAsync(gasto.IDGASTO);
            }
            var participantes = await _repoParticipantes.GetByActividadIdAsync(idActividad);
            foreach (var p in participantes) await _repoParticipantes.DeleteAsync(p.IDPARTICIPANTE);

            await _repoActividades.DeleteAsync(idActividad);
            return RedirectToAction("Index", "Statics");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGasto(int idGasto)
        {
            Gasto gasto = await _repoGastos.GetByIdAsync(idGasto);
            if (gasto != null)
            {
                int idActividad = gasto.IDACTIVIDAD;
                var repartos = await _repoRepartos.GetRepartosByGastoAsync(idGasto);
                foreach (var rep in repartos) await _repoRepartos.DeleteAsync(rep.IdReparto);
                await _repoGastos.DeleteAsync(idGasto);
                return RedirectToAction("Detalle", new { id = idActividad });
            }
            return RedirectToAction("Index", "Statics");
        }

        [HttpPost]
        public async Task<IActionResult> AbandonarActividad(int idActividad)
        {
            Usuario user = HttpContext.Session.getObject<Usuario>("USUARIO_LOGUEADO");
            var participantes = await _repoParticipantes.GetByActividadIdAsync(idActividad);
            var miParticipante = participantes.FirstOrDefault(p => p.IDUSUARIO == user.IDUSUARIO);

            if (miParticipante != null)
            {
                miParticipante.IDUSUARIO = null;
                await _repoParticipantes.UpdateAsync(miParticipante);
            }
            return RedirectToAction("Index", "Statics");
        }
    }
}