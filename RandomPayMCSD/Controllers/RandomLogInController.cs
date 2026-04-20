using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RandomPayMCSD.Extensions;
using RandomPayMCSD.Models;
using RandomPayMCSD.Services;
using System.Security.Claims;

namespace RandomPayMCSD.Controllers
{
    public class RandomLogInController : Controller
    {
        private readonly AuthApiService _authApiService;
        private readonly UsuarioApiService _usuarioApiService;

        public RandomLogInController(AuthApiService authApiService, UsuarioApiService usuarioApiService)
        {
            _authApiService = authApiService;
            _usuarioApiService = usuarioApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Usuario? userSession = HttpContext.Session.getObject<Usuario>("USUARIO_LOGUEADO");

            if (User.Identity?.IsAuthenticated == true && userSession != null)
            {
                return RedirectToAction("Index", "Statics");
            }

            if (User.Identity?.IsAuthenticated == true && userSession == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            var (usuario, token) = await _authApiService.LoginAsync(email, password);

            if (usuario != null)
            {
                ClaimsIdentity identity = new ClaimsIdentity(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    ClaimTypes.Name,
                    ClaimTypes.Role);

                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, usuario.IDUSUARIO.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Name, usuario.NOMBRE));
                identity.AddClaim(new Claim(ClaimTypes.Email, usuario.EMAIL));
                identity.AddClaim(new Claim(ClaimTypes.Role, string.IsNullOrWhiteSpace(usuario.ROL) ? "USER" : usuario.ROL));

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                HttpContext.Session.setObject("USUARIO_LOGUEADO", usuario);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    HttpContext.Session.SetString("JWT_TOKEN", token);
                }

                return RedirectToAction("Index", "Statics");
            }

            ViewData["MENSAJE"] = "Email o contraseña incorrectos.";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string nombre, string email, string password)
        {
            nombre = nombre?.Trim() ?? string.Empty;
            email = (email ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["MENSAJE"] = "Debes rellenar nombre, email y contraseña.";
                return View();
            }

            try
            {
                await _usuarioApiService.RegisterAsync(nombre, email, password);
                TempData["MENSAJE_EXITO"] = "Cuenta creada correctamente. ¡Inicia sesión!";
                return RedirectToAction("Index");
            }
            catch
            {
                ViewData["MENSAJE"] = "No se pudo completar el registro.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                await _usuarioApiService.ForgotPasswordAsync(email);
            }
            catch
            {
            }

            ViewData["MENSAJE_INFO"] = "Si el correo está registrado, recibirás un enlace para cambiar tu contraseña.";
            return View();
        }

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
            try
            {
                await _usuarioApiService.ResetPasswordAsync(email, token, newPassword);
                TempData["MENSAJE_EXITO"] = "Contraseña restablecida correctamente. Ya puedes iniciar sesión.";
                return RedirectToAction("Index");
            }
            catch
            {
                ViewData["MENSAJE_ERROR"] = "El enlace no es válido o ha caducado. Vuelve a solicitar la recuperación.";
                return View();
            }
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("USUARIO_LOGUEADO");
            HttpContext.Session.Remove("JWT_TOKEN");
            return RedirectToAction("Index", "RandomLogIn");
        }

        public IActionResult ErrorAcceso()
        {
            return View();
        }
    }
}