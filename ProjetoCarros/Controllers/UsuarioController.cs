using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using ProjetoCarros.Models;
using ProjetoCarros.Repositorio;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using ProjetoCarros.Interfaces;
namespace ProjetoCarros.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        public UsuarioController(IUsuarioRepositorio usuarioRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
        }
        [HttpGet]
        public IActionResult Logar() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logar(Usuario model)
        {
            if (!ModelState.IsValid) return View(model);
            var usuario = _usuarioRepositorio.Validar(model.Email, model.Senha);
            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim("NivelAcesso",usuario.Nivel),
                    new Claim("UsuarioId",usuario.Id.ToString())
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties { IsPersistent = false });
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "E-mail ou senha Invalidos.");
            return View(model);
        }

        [HttpGet]
        public IActionResult CriarConta() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CriarConta(Usuario model)
        {
            if (!ModelState.IsValid) return View(model);
            _usuarioRepositorio.CriarConta(model);
            return RedirectToAction("Logar");
        }

        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public  IActionResult DeletarConta()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletarConta(ConfirmarExclusaoViewModel model)
        {
            var usuarioIdClaim = User.FindFirst("UsuarioId");
            if (usuarioIdClaim == null)
            {
                return RedirectToAction("Logar");
            }
            int usuarioId = int.Parse(usuarioIdClaim.Value);
            var usuario = _usuarioRepositorio.BuscarPorId(usuarioId);
            if (usuario == null)
            {
                return RedirectToAction("Logar");
            }

            bool senhaValida = BCrypt.Net.BCrypt.Verify(model.Senha, usuario.Senha);
            if (!senhaValida)
            {
                ModelState.AddModelError("", "Senha Incorreta.");
                return View(model);
            }
            _usuarioRepositorio.DeletarConta(usuarioId);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Logar");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Configuracoes()
        {
            var usuarioId = int.Parse(
                User.FindFirst("UsuarioId").Value);

            var usuario = _usuarioRepositorio.BuscarPorId(usuarioId);
            return View(usuario);
        }

        [HttpPost]

        // Faz com que a claims seja resetada com as novas informaçoes
        public async Task<IActionResult> EditarPerfil(Usuario usuario)
        {
            _usuarioRepositorio.Atualizar(usuario);

            var usuarioAtualizado =
                _usuarioRepositorio.BuscarPorId(usuario.Id);

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuarioAtualizado.Nome),
        new Claim(ClaimTypes.Email, usuarioAtualizado.Email),
        new Claim("NivelAcesso", usuarioAtualizado.Nivel),
        new Claim("UsuarioId", usuarioAtualizado.Id.ToString())
    };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Configuracoes");
        }
        // ---- LOGIN VIA AJAX (usado no passo 2 do agendamento de test-drive) ----
        [HttpPost]
        public async Task<IActionResult> LogarAjax([FromBody] Usuario model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Senha))
            {
                return Json(new { sucesso = false, mensagem = "Preencha e-mail e senha." });
            }

            var usuario = _usuarioRepositorio.Validar(model.Email, model.Senha);
            if (usuario == null)
            {
                return Json(new { sucesso = false, mensagem = "E-mail ou senha inválidos." });
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.Nome),
        new Claim(ClaimTypes.Email, usuario.Email),
        new Claim("NivelAcesso", usuario.Nivel),
        new Claim("UsuarioId", usuario.Id.ToString())
    };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties { IsPersistent = false });

            return Json(new { sucesso = true, nome = usuario.Nome });
        }
    }
}