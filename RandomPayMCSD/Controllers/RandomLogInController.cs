using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Helpers;
using RandomPayMCSD.Data;
using RandomPayMCSD.Extensions;
using RandomPayMCSD.Helpers;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace RandomPayMCSD.Controllers
{
    public class RandomLogInController : Controller
    {
        private IRepositoryUsuarios repo;
        private RandomPayContext context;
        private readonly IConfiguration _config;

        // Inyectamos el Repo, el Context de BD y la Configuración (appsettings.json)
        public RandomLogInController(IRepositoryUsuarios repo, RandomPayContext context, IConfiguration config)
        {
            this.repo = repo;
            this.context = context;
            this._config = config;
        }

        // --- LOGIN ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Usuario userSession = HttpContext.Session.getObject<Usuario>("USUARIO_LOGUEADO");

            if (User.Identity.IsAuthenticated && userSession != null)
            {
                return RedirectToAction("Index", "Statics");
            }

            if (User.Identity.IsAuthenticated && userSession == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            Usuario usuario = await this.repo.GetByEmailAsync(email);

            if (usuario != null)
            {
                SeguridadUsuario seguridad = await this.context.SeguridadUsuarios
                    .FirstOrDefaultAsync(x => x.IdUsuario == usuario.IDUSUARIO);

                if (seguridad != null)
                {
                    byte[] hashLogin = HelperCryptography.EncryptPassword(password, seguridad.Salt);
                    bool passCorrecta = HelperTools.CompareArrays(hashLogin, seguridad.PasswordHash);

                    if (passCorrecta)
                    {
                        ClaimsIdentity identity = new ClaimsIdentity(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            ClaimTypes.Name, ClaimTypes.Role);

                        Claim claimId = new Claim(ClaimTypes.NameIdentifier, usuario.IDUSUARIO.ToString());
                        Claim claimName = new Claim(ClaimTypes.Name, usuario.NOMBRE);
                        Claim claimEmail = new Claim(ClaimTypes.Email, usuario.EMAIL);
                        Claim claimRole = new Claim(ClaimTypes.Role, usuario.ROL ?? "USER");

                        identity.AddClaim(claimId);
                        identity.AddClaim(claimName);
                        identity.AddClaim(claimEmail);
                        identity.AddClaim(claimRole);

                        ClaimsPrincipal userPrincipal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);
                        HttpContext.Session.setObject("USUARIO_LOGUEADO", usuario);

                        return RedirectToAction("Index", "Statics");
                    }
                }
            }

            ViewData["MENSAJE"] = "Email o contraseña incorrectos.";
            return View();
        }

        // --- REGISTER ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string nombre, string email, string password)
        {
            nombre = nombre?.Trim();
            email = email?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["MENSAJE"] = "Debes rellenar nombre, email y contraseña.";
                return View();
            }

            Usuario existe = await this.repo.GetByEmailAsync(email);
            if (existe != null)
            {
                ViewData["MENSAJE"] = "Este email ya está registrado.";
                return View();
            }

            using var tx = await this.context.Database.BeginTransactionAsync();
            try
            {
                Usuario nuevoUsuario = new Usuario
                {
                    NOMBRE = nombre,
                    EMAIL = email,
                    PASSWORD = password, // Guardado en texto plano temporalmente para tus pruebas
                    ROL = "USER"
                };

                await this.repo.AddAsync(nuevoUsuario);

                string salt = HelperTools.GenerateSalt();
                byte[] hash = HelperCryptography.EncryptPassword(password, salt);

                SeguridadUsuario seguridad = new SeguridadUsuario
                {
                    IdUsuario = nuevoUsuario.IDUSUARIO,
                    Salt = salt,
                    PasswordHash = hash
                };

                await this.context.SeguridadUsuarios.AddAsync(seguridad);
                await this.context.SaveChangesAsync();

                await tx.CommitAsync();

                TempData["MENSAJE_EXITO"] = "Cuenta creada correctamente. ¡Inicia sesión!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                ViewData["MENSAJE"] = "No se pudo completar el registro por un problema de base de datos.";
                return View();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                ViewData["MENSAJE"] = "No se pudo completar el registro. Inténtalo de nuevo.";
                return View();
            }
        }

        // --- FORGOT PASSWORD ---
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            Usuario usuario = await this.repo.GetByEmailAsync(email);

            if (usuario != null)
            {
                SeguridadUsuario seguridad = await this.context.SeguridadUsuarios
                    .FirstOrDefaultAsync(x => x.IdUsuario == usuario.IDUSUARIO);

                if (seguridad != null)
                {
                    string token = Guid.NewGuid().ToString();
                    seguridad.TokenRecuperacion = token;
                    seguridad.FechaExpiracionToken = DateTime.Now.AddHours(1);
                    await this.context.SaveChangesAsync();

                    string urlRecuperacion = Url.Action("ResetPassword", "RandomLogIn",
                        new { email = usuario.EMAIL, token = token }, Request.Scheme);

                    try
                    {
                        await EnviarCorreoRecuperacionAsync(usuario.EMAIL, urlRecuperacion);
                        ViewData["MENSAJE_INFO"] = "Si el correo está registrado, recibirás un enlace para cambiar tu contraseña.";
                    }
                    catch (Exception ex)
                    {
                        ViewData["MENSAJE_ERROR"] = "Error al enviar: " + ex.Message;
                        return View();
                    }

                    return View();
                }
            }

            ViewData["MENSAJE_INFO"] = "Si el correo está registrado, recibirás un enlace para cambiar tu contraseña.";
            return View();
        }

        // --- RESET PASSWORD ---
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string token, string newPassword)
        {
            Usuario usuario = await this.repo.GetByEmailAsync(email);

            if (usuario != null)
            {
                SeguridadUsuario seguridad = await this.context.SeguridadUsuarios
                    .FirstOrDefaultAsync(x => x.IdUsuario == usuario.IDUSUARIO);

                if (seguridad != null && seguridad.TokenRecuperacion == token && seguridad.FechaExpiracionToken > DateTime.Now)
                {
                    // Actualizamos la pass antigua
                    usuario.PASSWORD = newPassword;
                    await this.repo.UpdateAsync(usuario);

                    // Generamos nuevo Hash y Salt
                    string nuevoSalt = HelperTools.GenerateSalt();
                    byte[] nuevoHash = HelperCryptography.EncryptPassword(newPassword, nuevoSalt);

                    seguridad.Salt = nuevoSalt;
                    seguridad.PasswordHash = nuevoHash;
                    seguridad.TokenRecuperacion = null;
                    seguridad.FechaExpiracionToken = null;

                    await this.context.SaveChangesAsync();

                    TempData["MENSAJE_EXITO"] = "Contraseña restablecida correctamente. Ya puedes iniciar sesión.";
                    return RedirectToAction("Index");
                }
            }

            ViewData["MENSAJE_ERROR"] = "El enlace no es válido o ha caducado. Vuelve a solicitar la recuperación.";
            return View();
        }

        // --- LOGOUT ---
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("USUARIO_LOGUEADO");

            return RedirectToAction("Index", "RandomLogIn");
        }

        // --- ERROR ACCESO ---
        public IActionResult ErrorAcceso()
        {
            return View();
        }

        // --- ENVÍO DE CORREO ---
        private async Task EnviarCorreoRecuperacionAsync(string emailDestino, string enlace)
        {
            // Leemos de appsettings.Development.json
            string miCorreo = _config["EmailSettings:Correo"];
            string miPassword = _config["EmailSettings:Password"];

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(miCorreo, "Equipo de RandomPay");
            mail.To.Add(emailDestino);
            mail.Subject = "Recuperación de contraseña - RandomPay";
            mail.Body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2 style='color: #2563eb;'>Recuperación de Contraseña</h2>
                    <p>Has solicitado restablecer tu contraseña en RandomPay.</p>
                    <p>Haz clic en el siguiente botón para crear una nueva:</p>
                    <a href='{enlace}' style='display: inline-block; padding: 10px 20px; background-color: #2563eb; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                        Cambiar Contraseña
                    </a>
                    <p style='margin-top: 20px; font-size: 12px; color: #666;'>Si no fuiste tú, ignora este correo. Este enlace caducará en 1 hora.</p>
                </div>";
            mail.IsBodyHtml = true;

            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential(miCorreo, miPassword);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
        }
    }
}