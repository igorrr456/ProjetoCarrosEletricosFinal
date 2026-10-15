using Microsoft.AspNetCore.Mvc;

namespace ProjetoCarros.Controllers
{
    public class ServicosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Agendamentos()
        {
            return View();
        }
        public IActionResult Agendar()
        {
            return View();
        }
        public IActionResult Financiamento()
        {
            return View();
        }
        public IActionResult TestDrive()
        {
            return View();
        }
        public IActionResult Carregamento()
        {
            return View();
        }
    }
}
