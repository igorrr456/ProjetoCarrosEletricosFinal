using ProjetoCarros.Models;

namespace ProjetoCarros.Interfaces
{
    public interface ICarroRepositorio
    {
        List<Carro> ListarTodos();
        List<Carro> ListarHibridos();
        List<Carro> ListarEletricos();
        Carro? BuscarPorId(int Id);
        void Criar(Carro carro, IFormFile? imagem);
        void Editar(Carro carro, IFormFile? imagem);
        void Deletar(int id);
    }
}
