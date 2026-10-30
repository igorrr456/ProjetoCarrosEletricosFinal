using System.ComponentModel.DataAnnotations;

namespace ProjetoCarros.Models
{
    public class Carro
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A marca é obrigatória")]
        public int IdMarca { get; set; }

        // Nome da marca para exibição nas Views
        public string? Marca { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public int IdCategoria { get; set; }

        // Nome da categoria para exibição nas Views
        public string? Categoria { get; set; }

        [Required(ErrorMessage = "O ano é obrigatório")]
        public int Ano { get; set; }

        public string? Descricao { get; set; }

        public string? Imagem { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        public decimal Preco { get; set; }

        public DateTime DataCadastro { get; set; }
    }
}