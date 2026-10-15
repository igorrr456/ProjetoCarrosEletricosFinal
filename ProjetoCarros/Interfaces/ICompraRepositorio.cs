using ProjetoCarros.Models;

namespace ProjetoCarros.Interfaces
{
    public interface ICompraRepositorio
    {
        Compra Criar(Compra compra);
        Compra BuscarPorId(int id);
    }
}
