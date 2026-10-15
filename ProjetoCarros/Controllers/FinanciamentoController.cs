using Microsoft.AspNetCore.Mvc;
using ProjetoCarros.Interfaces;

namespace ProjetoCarros.Controllers
{
    public class FinanciamentoController : Controller
    {
        private readonly ICarroRepositorio _carroRepositorio;

        public FinanciamentoController(ICarroRepositorio carroRepositorio)
        {
            _carroRepositorio = carroRepositorio;
        }

        // GET /Financiamento
        // GET /Financiamento?id=5  -> já vem com o carro pré-selecionado
        // (útil se um dia você colocar um botão "Financiar" no anúncio do carro)
        public IActionResult Index(int? id)
        {
            var carros = _carroRepositorio.ListarTodos();
            ViewBag.CarroSelecionadoId = id;
            return View(carros);
        }
    }
}
