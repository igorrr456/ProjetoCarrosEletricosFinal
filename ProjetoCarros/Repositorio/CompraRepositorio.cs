using MySql.Data.MySqlClient;
using ProjetoCarros.Interfaces;
using ProjetoCarros.Models;

namespace ProjetoCarros.Repositorio
{
    public class CompraRepositorio : ICompraRepositorio
    {
        private readonly string _connectionString;
        public CompraRepositorio(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Conexao");
        }

        public Compra Criar(Compra compra)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = @"INSERT INTO tb_compras
            (Id_usuario, Id_carro, Nome_cliente, Nome_carro, Categoria, Imagem, Valor, Data_retirada)
            VALUES (@idUsuario, @idCarro, @nomeCliente, @nomeCarro, @categoria, @imagem, @valor, @dataRetirada)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idUsuario", compra.IdUsuario);
            cmd.Parameters.AddWithValue("@idCarro", compra.IdCarro);
            cmd.Parameters.AddWithValue("@nomeCliente", compra.NomeCliente);
            cmd.Parameters.AddWithValue("@nomeCarro", compra.NomeCarro);
            cmd.Parameters.AddWithValue("@categoria", compra.Categoria);
            cmd.Parameters.AddWithValue("@imagem", compra.Imagem);
            cmd.Parameters.AddWithValue("@valor", compra.Valor);
            cmd.Parameters.AddWithValue("@dataRetirada", compra.DataRetirada);
            cmd.ExecuteNonQuery();

            //vai pegar o id gerado pelo auto increment
            compra.Id = (int)cmd.LastInsertedId;
            return compra;
        }

        public Compra BuscarPorId(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT *FROM tb_compras WHERE Id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using (var reader = cmd.ExecuteReader())
            {
                if(reader.Read())
                {
                    return new Compra
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        IdUsuario = Convert.ToInt32(reader["Id_usuario"]),
                        IdCarro = Convert.ToInt32(reader["Id_carro"]),
                        NomeCliente = reader["Nome_cliente"].ToString()!,
                        NomeCarro = reader["Nome_carro"].ToString()!,
                        Categoria = reader["Categoria"].ToString()!,
                        Imagem = reader["Imagem"]?.ToString(),
                        Valor = Convert.ToDecimal(reader["Valor"]),
                        DataCompra = Convert.ToDateTime(reader["Data_compra"]),
                        DataRetirada = Convert.ToDateTime(reader["Data_retirada"])
                    };
                    
                }
                return null;
            }
        }
    }
}
