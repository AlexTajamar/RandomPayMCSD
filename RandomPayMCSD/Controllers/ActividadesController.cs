using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RandomPayMCSD.Extensions;
using RandomPayMCSD.Interfaces;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace RandomPayMCSD.Controllers
{
    [Authorize]
    public class ActividadesController : Controller
    {
        private readonly IRepositoryActividades _repoActividades;
        private readonly IRepositoryGastos _repoGastos;
        private readonly IRepositoryParticipantes _repoParticipantes;
        private readonly IRepositoryRepartos _repoRepartos;
        private readonly IRepositoryDivisas _repoDivisas;
        private readonly IRepositoryListaCompra _repoListaCompra;
        private readonly BalanceService _balanceService;
        private readonly InvitationService _invitationService;
        private readonly IConfiguration _config;
        private readonly ILogger<ActividadesController> _logger;
        public ActividadesController(
            IRepositoryActividades repoActividades,
            IRepositoryListaCompra repoListaCompra,
            IRepositoryGastos repoGastos,
            IRepositoryDivisas repoDivisas,
            IRepositoryParticipantes repoParticipantes,
            IRepositoryRepartos repoRepartos,
            BalanceService balanceService,
            InvitationService invitationService,
            IConfiguration config,
            ILogger<ActividadesController> logger)
        {
            _repoActividades = repoActividades;
            _repoGastos = repoGastos;
            _repoParticipantes = repoParticipantes;
            _balanceService = balanceService;
            _invitationService = invitationService;
            _repoRepartos = repoRepartos;
            _repoDivisas = repoDivisas;
            _repoListaCompra = repoListaCompra;
            _config = config;
            _logger = logger;
        }

        public async Task<IActionResult> Detalle(int id)
        {
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(id);
            if (actividad == null) return RedirectToAction("Index", "Statics");

            if (actividad.Gastos != null && actividad.Participantes != null)
            {
                foreach (var gasto in actividad.Gastos)
                {
                    if (gasto.Pagador == null)
                    {
                        gasto.Pagador = actividad.Participantes.FirstOrDefault(p => p.IDPARTICIPANTE == gasto.IDPAGADOR);
                    }
                }
            }

            ViewBag.Balances = await _balanceService.GetBalancesActividadAsync(id);
            ViewBag.Transferencias = await _balanceService.GetTransferenciasAsync(id);
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
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string nombreUsuario = User.FindFirstValue(ClaimTypes.Name);

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
                IDCREADOR = idUsuario,
                FECHACREACION = DateTime.Now,
                INVITACIONCOD = _invitationService.GenerarCodigoUnico(),
                IMAGEN = imagenFinal
            };
            int idActividadCreada = await _repoActividades.AddAsync(nuevaActividad);
            if (idActividadCreada <= 0)
            {
                TempData["ERROR_CORREO"] = "No se pudo recuperar la actividad creada.";
                return RedirectToAction("Index", "Statics");
            }

            Participante creadorParticipante = new Participante
            {
                IDACTIVIDAD = idActividadCreada,
                NOMBREPARTICIPANTE = nombreUsuario,
                IDUSUARIO = idUsuario
            };
            await _repoParticipantes.AddAsync(creadorParticipante);

            if (nombresAmigos != null)
            {
                foreach (var nombreAmigo in nombresAmigos.Where(n => !string.IsNullOrWhiteSpace(n)))
                {
                    await _repoParticipantes.AddAsync(new Participante
                    {
                        IDACTIVIDAD = idActividadCreada,
                        NOMBREPARTICIPANTE = nombreAmigo.Trim(),
                        IDUSUARIO = null
                    });
                }
            }

            return RedirectToAction("Detalle", new { id = idActividadCreada });
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

        [HttpGet]
        public IActionResult Unirse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Unirse(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return View();

            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var actividad = await _repoActividades.GetByCodigoAsync(codigo);

            if (actividad == null)
            {
                ViewBag.Error = "No existe ninguna actividad con el código: " + codigo.ToUpper();
                return View();
            }

            if (actividad.Participantes.Any(p => p.IDUSUARIO == idUsuario))
            {
                ViewBag.Error = "Ya estás dentro de esta actividad. Búscala en tu panel de inicio.";
                return View();
            }

            return RedirectToAction("Vincular", new { id = actividad.IDACTIVIDAD });
        }

        [HttpGet]
        public async Task<IActionResult> Vincular(int id)
        {
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(id);

            if (actividad == null) return RedirectToAction("Index", "Statics");

            if (actividad.Participantes.Any(p => p.IDUSUARIO == idUsuario))
            {
                return RedirectToAction("Detalle", new { id = id });
            }

            ViewBag.Huerfanos = actividad.Participantes.Where(p => p.IDUSUARIO == null).ToList();
            return View(actividad);
        }

        [HttpPost]
        public async Task<IActionResult> Vincular(int idActividad, int idParticipante)
        {
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Participante p = await _repoParticipantes.GetByIdAsync(idParticipante);

            if (p != null)
            {
                p.IDUSUARIO = idUsuario;
                await _repoParticipantes.UpdateAsync(p);
            }

            return RedirectToAction("Detalle", new { id = idActividad });
        }

        [HttpPost]
        public async Task<IActionResult> VincularNuevo(int idActividad)
        {
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string nombreUsuario = User.FindFirstValue(ClaimTypes.Name);

            await _repoParticipantes.AddAsync(new Participante
            {
                IDACTIVIDAD = idActividad,
                NOMBREPARTICIPANTE = nombreUsuario,
                IDUSUARIO = idUsuario
            });

            return RedirectToAction("Detalle", new { id = idActividad });
        }

        [HttpPost]
        public async Task<IActionResult> AddItemCompra(int idActividad, string nombreItem, string precioEstimadoStr)
        {
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
            if (!TryParseFlexibleDecimal(importeString, out decimal totalImporte) || totalImporte <= 0)
            {
                ModelState.AddModelError("IMPORTE", "Importe no válido.");
                ViewBag.Participantes = await _repoParticipantes.GetByActividadIdAsync(gastoNormal.IDACTIVIDAD);
                ViewBag.Divisas = await _repoDivisas.GetDivisasAsync();
                return View(gastoNormal);
            }

            gastoNormal.IMPORTE = Math.Round(totalImporte, 2);

            if (divisaSeleccionada != "EUR")
            {
                var moneda = await _repoDivisas.GetDivisaByCodigoAsync(divisaSeleccionada);
                if (moneda != null) gastoNormal.IMPORTE = Math.Round(gastoNormal.IMPORTE / (decimal)moneda.Tasa, 2);
            }

            gastoNormal.FECHA = DateTime.Now;
            int idGastoCreado = await _repoGastos.AddAsync(gastoNormal);
            if (idGastoCreado <= 0)
            {
                TempData["ERROR_CORREO"] = "Se creó el gasto, pero no se pudo recuperar su identificador para repartirlo.";
                return RedirectToAction("Detalle", new { id = gastoNormal.IDACTIVIDAD });
            }

            gastoNormal.IDGASTO = idGastoCreado;

            if (idsParticipantes != null)
            {
                for (int i = 0; i < idsParticipantes.Count; i++)
                {
                    if (i < cantidadesAsignadas.Count && TryParseFlexibleDecimal(cantidadesAsignadas[i], out decimal valorDecimal) && valorDecimal > 0)
                    {
                        double valorRedondeado = (double)Math.Round(valorDecimal, 2);
                        if (divisaSeleccionada != "EUR")
                        {
                            var moneda = await _repoDivisas.GetDivisaByCodigoAsync(divisaSeleccionada);
                            if (moneda != null)
                            {
                                valorRedondeado = Math.Round(valorRedondeado / moneda.Tasa, 2);
                            }
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
            if (!TryParseFlexibleDecimal(importeString, out decimal totalImporte) || totalImporte <= 0)
            {
                ModelState.AddModelError("IMPORTE", "Importe no válido.");
                ViewBag.Participantes = await _repoParticipantes.GetByActividadIdAsync(gastoEditado.IDACTIVIDAD);
                ViewBag.Divisas = await _repoDivisas.GetDivisasAsync();
                ViewBag.Repartos = await _repoRepartos.GetRepartosByGastoAsync(gastoEditado.IDGASTO);
                return View("AddGasto", gastoEditado);
            }

            gastoEditado.IMPORTE = Math.Round(totalImporte, 2);

            if (divisaSeleccionada != "EUR")
            {
                var moneda = await _repoDivisas.GetDivisaByCodigoAsync(divisaSeleccionada);
                if (moneda != null)
                {
                    gastoEditado.IMPORTE = Math.Round(gastoEditado.IMPORTE / (decimal)moneda.Tasa, 2);
                }
            }

            await _repoGastos.UpdateAsync(gastoEditado);

            var antiguos = await _repoRepartos.GetRepartosByGastoAsync(gastoEditado.IDGASTO);
            foreach (var r in antiguos) await _repoRepartos.DeleteAsync(r.IdReparto);

            if (idsParticipantes != null)
            {
                for (int i = 0; i < idsParticipantes.Count; i++)
                {
                    if (i < cantidadesAsignadas.Count && TryParseFlexibleDecimal(cantidadesAsignadas[i], out decimal valorDecimal) && valorDecimal > 0)
                    {
                        double valorRedondeado = (double)Math.Round(valorDecimal, 2);

                        if (divisaSeleccionada != "EUR")
                        {
                            var moneda = await _repoDivisas.GetDivisaByCodigoAsync(divisaSeleccionada);
                            if (moneda != null)
                            {
                                valorRedondeado = Math.Round(valorRedondeado / moneda.Tasa, 2);
                            }
                        }

                        await _repoRepartos.AddAsync(new RepartoGasto
                        {
                            IdGasto = gastoEditado.IDGASTO,
                            IdParticipante = idsParticipantes[i],
                            Cantidad = valorRedondeado
                        });
                    }
                }
            }
            return RedirectToAction("Detalle", new { id = gastoEditado.IDACTIVIDAD });
        }

        [HttpPost]
        public async Task<IActionResult> SaldarDeuda(int idActividad, int idDeudor, int idAcreedor, string cantidadString)
        {
            if (TryParseFlexibleDecimal(cantidadString, out decimal cantidadDecimal))
            {
                cantidadDecimal = Math.Round(cantidadDecimal, 2, MidpointRounding.AwayFromZero);

                if (cantidadDecimal > 0.01m)
                {
                    Gasto reembolso = new Gasto
                    {
                        IDACTIVIDAD = idActividad,
                        IDPAGADOR = idDeudor,
                        CONCEPTO = "💸 Reembolso de deuda",
                        IMPORTE = cantidadDecimal,
                        FECHA = DateTime.Now
                    };

                    int idReembolso = await _repoGastos.AddAsync(reembolso);
                    if (idReembolso <= 0)
                    {
                        TempData["ERROR_CORREO"] = "No se pudo registrar el reembolso correctamente.";
                        return RedirectToAction("Detalle", new { id = idActividad });
                    }

                    reembolso.IDGASTO = idReembolso;

                    await _repoRepartos.AddAsync(new RepartoGasto
                    {
                        IdGasto = reembolso.IDGASTO,
                        IdParticipante = idAcreedor,
                        Cantidad = (double)cantidadDecimal
                    });
                }
            }
            return RedirectToAction("Detalle", new { id = idActividad });
        }

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
                foreach (var rep in repartos)
                {
                    await _repoRepartos.DeleteAsync(rep.IdReparto);
                }

                var itemsCompra = await _repoListaCompra.GetByActividadAsync(idActividad);
                var itemsDelGasto = itemsCompra.Where(i => i.IdGasto == idGasto).ToList();
                foreach (var item in itemsDelGasto)
                {
                    item.IdGasto = null;
                    item.Comprado = false;
                    await _repoListaCompra.UpdateAsync(item);
                }

                await _repoGastos.DeleteAsync(idGasto);

                return RedirectToAction("Detalle", new { id = idActividad });
            }
            return RedirectToAction("Index", "Statics");
        }

        [HttpPost]
        public async Task<IActionResult> AbandonarActividad(int idActividad)
        {
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var participantes = await _repoParticipantes.GetByActividadIdAsync(idActividad);
            var miParticipante = participantes.FirstOrDefault(p => p.IDUSUARIO == idUsuario);

            if (miParticipante != null)
            {
                miParticipante.IDUSUARIO = null;
                await _repoParticipantes.UpdateAsync(miParticipante);
            }
            return RedirectToAction("Index", "Statics");
        }

        [HttpPost]
        public async Task<IActionResult> SendDebtReminderEmail(int idActividad, int idDeudor, int idAcreedor)
        {
            try
            {
                Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(idActividad);
                if (actividad == null)
                {
                    TempData["ERROR_CORREO"] = "No se encontró la actividad.";
                    return RedirectToAction("Detalle", new { id = idActividad });
                }

                var deudor = actividad.Participantes.FirstOrDefault(p => p.IDPARTICIPANTE == idDeudor);
                if (deudor?.IDUSUARIO == null)
                {
                    TempData["ERROR_CORREO"] = "El deudor no tiene email registrado.";
                    return RedirectToAction("Detalle", new { id = idActividad });
                }

                Usuario usuarioDeudor = null;
                try
                {
                    usuarioDeudor = await _repoActividades.GetUsuarioByIdAsync(deudor.IDUSUARIO.Value);
                }
                catch (HttpRequestException)
                {
                    TempData["ERROR_CORREO"] = "No se pudo encontrar la cuenta del deudor en la base de datos (API devolvió 404).";
                    return RedirectToAction("Detalle", new { id = idActividad });
                }

                if (usuarioDeudor == null || string.IsNullOrEmpty(usuarioDeudor.EMAIL))
                {
                    TempData["ERROR_CORREO"] = "No se pudo obtener el email del deudor.";
                    return RedirectToAction("Detalle", new { id = idActividad });
                }

                var acreedor = actividad.Participantes.FirstOrDefault(p => p.IDPARTICIPANTE == idAcreedor);
                if (acreedor == null)
                {
                    TempData["ERROR_CORREO"] = "No se encontró el acreedor.";
                    return RedirectToAction("Detalle", new { id = idActividad });
                }

                if (acreedor.IDUSUARIO == null)
                {
                    TempData["ERROR_CORREO"] = "El acreedor no tiene email registrado.";
                    return RedirectToAction("Detalle", new { id = idActividad });
                }

                Usuario usuarioAcreedor = null;
                try
                {
                    usuarioAcreedor = await _repoActividades.GetUsuarioByIdAsync(acreedor.IDUSUARIO.Value);
                }
                catch (HttpRequestException)
                {
                    TempData["ERROR_CORREO"] = "No se pudo encontrar la cuenta del acreedor en la base de datos (API devolvió 404).";
                    return RedirectToAction("Detalle", new { id = idActividad });
                }

                if (usuarioAcreedor == null || string.IsNullOrEmpty(usuarioAcreedor.EMAIL))
                {
                    TempData["ERROR_CORREO"] = "No se pudo obtener el email del acreedor.";
                    return RedirectToAction("Detalle", new { id = idActividad });
                }

                var transferencias = await _balanceService.GetTransferenciasAsync(idActividad);
                var deudaEspecifica = transferencias.FirstOrDefault(t => t.IdDeudor == idDeudor && t.IdAcreedor == idAcreedor);
                string cantidadTexto = deudaEspecifica != null ? deudaEspecifica.Cantidad.ToString("N2") : "una cantidad pendiente";

                string asunto = "Recordatorio de Deuda en Actividad " + actividad.NOMBREACTIVIDAD;
                string urlApp = "https://randompay-cef4d0hudeh5cnay.germanywestcentral-01.azurewebsites.net";

                string cuerpo = $@"<div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                                            <h2 style='color: #2563eb;'>¡Hola {deudor.NOMBREPARTICIPANTE}!</h2>
                                            <p><strong>{acreedor.NOMBREPARTICIPANTE}</strong> te ha enviado un recordatorio desde RandomPay.</p>
                                            <p>Tienes una deuda pendiente de <strong>{cantidadTexto} {actividad.MONEDAPRINCIPAL}</strong> en la actividad <strong>{actividad.NOMBREACTIVIDAD}</strong>.</p>
                                            <p>Si aún no te has unido o necesitas registrar el pago, utiliza el código de invitación: <strong>{actividad.INVITACIONCOD}</strong></p>
        
                                            <div style='text-align: center; margin: 30px 0;'>
                                                <a href='{urlApp}' style='background-color: #2563eb; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                                                    Ver mi deuda en RandomPay
                                                </a>
                                            </div>

                                            <p>Por favor, realiza el pago correspondiente y avisa a {acreedor.NOMBREPARTICIPANTE} para que salde la deuda en la app.</p>
                                            <br/>
                                            <hr style='border: none; border-top: 1px solid #e2e8f0; margin-bottom: 15px;' />
                                            <p style='font-size: 0.9em; color: #666;'>Gracias por usar RandomPay 💸</p>
                                        </div>";

                string senderAddress = _config["EmailSettings:Correo"] ?? string.Empty;
                string connectionString = _config["EmailSettings:Password"] ?? string.Empty;

                if (string.IsNullOrEmpty(senderAddress) || string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("No se encontraron las credenciales de correo o contraseña en la configuración. Revisa Azure Key Vault.");
                }

                var emailClient = new Azure.Communication.Email.EmailClient(connectionString);

                var emailContent = new Azure.Communication.Email.EmailContent(asunto)
                {
                    Html = cuerpo
                };

                var emailMessage = new Azure.Communication.Email.EmailMessage(
                    senderAddress: senderAddress,
                    recipientAddress: usuarioDeudor.EMAIL,
                    content: emailContent);

                await emailClient.SendAsync(Azure.WaitUntil.Started, emailMessage);

                TempData["EXITO_CORREO"] = "Recordatorio de deuda enviado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar recordatorio de deuda");
                TempData["ERROR_CORREO"] = "Error al enviar: " + ex.Message;
            }

            return RedirectToAction("Detalle", new { id = idActividad });
        }

        private static bool TryParseFlexibleDecimal(string? rawValue, out decimal result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(rawValue)) return false;

            if (decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return true;

            rawValue = rawValue.Replace(',', '.');

            return decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }
    }
}