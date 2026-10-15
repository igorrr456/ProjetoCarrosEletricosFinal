using ProjetoCarros.Models;

namespace ProjetoCarros.Interfaces
{
    public interface ICarroRepositorio
    {
        List<Carro> ListarTodos();
        Carro BuscarPorId(int Id);
        void Criar(Carro carro, IFormFile? imagem);
        void Editar(Carro carro, IFormFile? imagem);
        void Deletar(int id);
    }
}
