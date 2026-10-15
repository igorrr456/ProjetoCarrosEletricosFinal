using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoCarros.Interfaces;
using ProjetoCarros.Models;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace ProjetoCarros.Controllers
{
    public class CarrosController : Controller
    {
        private readonly ICarroRepositorio _carroRepositorio;
        private readonly ICompraRepositorio _compraRepositorio;

        public CarrosController(ICarroRepositorio carroRepositorio, ICompraRepositorio compraRepositorio)
        {
            _carroRepositorio = carroRepositorio;
            _compraRepositorio = compraRepositorio;
        }

        //paginas publicas 
        public IActionResult Index()
        {
            var carros = _carroRepositorio.ListarTodos();
            return View(carros); //passa a lista para a view
        }
        public IActionResult CarrosHibridos()
        {
            //Filtra só os hibridos no c# mesmo
            var hibridos = _carroRepositorio.ListarTodos()
                .Where(c => c.Categoria == "PHEV" || c.Categoria == "HEV" || c.Categoria == "MHEV")
                .ToList();
            return View(hibridos);
        }
        public IActionResult CarrosEletricos()
        {
            var eletricos = _carroRepositorio.ListarTodos()
                .Where(c => c.Categoria == "BEV" || c.Categoria == "FCEV")
                .ToList();
            return View(eletricos);
        }

        //Area que apenas os adms vão poder gerenciar

        [Authorize]
        public IActionResult Gerenciar()
        {
            //verifica manualmente se é adm 
            if (User.FindFirst("NivelAcesso")?.Value != "Admin")
                return RedirectToAction("AcessoNegado");

            var carros = _carroRepositorio.ListarTodos();
                return View(carros);
        }
        [Authorize]
        [HttpGet]
        public IActionResult Criar() => View();

        [Authorize]
        [HttpPost]
        public IActionResult Criar( Carro model, IFormFile? imagem)
        {
            if (User.FindFirst("NivelAcesso")?.Value != "Admin")
                return RedirectToAction("AcessoNegado");

            if (!ModelState.IsValid)
                
                return View(model);

            _carroRepositorio.Criar(model, imagem);
            TempData["Sucesso"] = "Carro cadstrado com sucesso!";
            return RedirectToAction("Gerenciar");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (User.FindFirst("NivelAcesso")?.Value != "Admin")
                return RedirectToAction("AcessoNegado");

            var carro = _carroRepositorio.BuscarPorId(id);
            if (carro == null)
                return NotFound();
            return View(carro);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Carro model, IFormFile? imagem)
        {
            if (User.FindFirst("NivelAcesso")?.Value != "Admin")
                return RedirectToAction("AcessoNegado");

            ModelState.Remove("Imagem");

            if (!ModelState.IsValid) 
                return View(model);

            _carroRepositorio.Editar(model, imagem);
            TempData["Sucesso"] = "Carro atualizado com sucesso!";
            return RedirectToAction("Gerenciar");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deletar(int id)
        {
            if (User.FindFirst("NivelAcesso")?.Value != "Admin")
                return RedirectToAction("AcessoNegado");

            _carroRepositorio.Deletar(id);
            TempData["Sucesso"] = "Carro Removido.";
            return RedirectToAction("Gerenciar");
        }

        public IActionResult AcessoNegado() => View();

        public IActionResult TodosModelos()
        {
            var carros = _carroRepositorio.ListarTodos();
            return View(carros);
        }
        public IActionResult Novos()
        {
            return View();
        }
        public IActionResult SemiNovos()
        {
            return View();
        }

       


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Comprar(int id)
        {
            var usuarioIdClaim = User.FindFirst("UsuarioId");
            if (usuarioIdClaim == null) return RedirectToAction("Logar", "Usuario");

            var carro = _carroRepositorio.BuscarPorId(id);
            if (carro == null) return NotFound();

            //calcula 3 dias uteis a partir de hoje
            DateTime dataRetirada = DateTime.Today;
            int diasAdicionados = 0;
            while (diasAdicionados < 3)
            {
                dataRetirada = dataRetirada.AddDays(1);
                if (dataRetirada.DayOfWeek != DayOfWeek.Saturday && dataRetirada.DayOfWeek != DayOfWeek.Sunday)
                {
                    diasAdicionados++;
                }

            }

            var compra = new Compra
            {
                IdUsuario = int.Parse(usuarioIdClaim.Value),
                IdCarro = carro.Id,
                NomeCliente = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Cliente",
                NomeCarro = carro.Nome,
                Categoria = carro.Categoria,
                Imagem = carro.Imagem,
                Valor = carro.Preco,
                DataRetirada = dataRetirada

            };

            var compraCriada = _compraRepositorio.Criar(compra);
            return RedirectToAction("Comprovante", new { id = compraCriada.Id });

        }


        [Authorize]
        public IActionResult Comprovante(int id)
        {
            var compra = _compraRepositorio.BuscarPorId(id);
            if (compra == null) return NotFound();

            //Garante que so o proprio comprador veja o comprovante
            var usuarioIdClaim = User.FindFirst("UsuarioId");
            if (usuarioIdClaim == null || compra.IdUsuario != int.Parse(usuarioIdClaim.Value))
                return RedirectToAction("AcessoNegado");

            return View(compra);
        }
    }
}
