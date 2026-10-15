using System.ComponentModel.DataAnnotations;

namespace ProjetoCarros.Models
{
    public class Carro
    {
        [Key]
        public int Id { get; set; }


        [Required(ErrorMessage ="O nome é obrigatório")]
        public string? Nome { get; set; }

        public string? Marca { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public string Categoria { get; set; } = string.Empty;
        public int Ano { get; set; }
        public string? Descricao { get; set; }
        public string? Imagem { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        public decimal Preco { get; set; }
        public DateTime DataCadastro { get; set; }

    }
}
