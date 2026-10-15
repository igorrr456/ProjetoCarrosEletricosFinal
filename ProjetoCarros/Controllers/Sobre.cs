using Microsoft.AspNetCore.Mvc;

namespace ProjetoCarros.Controllers
{
    public class Sobre : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult QuemSomos()
        {
            return View();
        }
        public IActionResult NossaMarca()
        {
            return View();
        }
        public IActionResult Contato()
        {
            return View();
        }
    }
}
