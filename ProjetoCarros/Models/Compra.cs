namespace ProjetoCarros.Models
{
    public class Compra
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public int IdCarro { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public string NomeCarro { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string ? Imagem { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataCompra { get; set; }
        public DateTime DataRetirada { get; set; }

    }
}
