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

            using var cmd = new MySqlCommand("sp_cadastrar_compra", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_id_usuario", compra.IdUsuario);
            cmd.Parameters.AddWithValue("@p_id_carro", compra.IdCarro);
            cmd.Parameters.AddWithValue("@p_valor", compra.Valor);
            cmd.Parameters.AddWithValue("@p_data_retirada", compra.DataRetirada);

            cmd.ExecuteNonQuery();

            //vai pegar o id gerado pelo auto increment
            compra.Id = (int)cmd.LastInsertedId;

            return compra;
        }

        public Compra? BuscarPorId(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            using var cmd = new MySqlCommand("sp_buscar_compra", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_id_compra", id);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Compra
                    {
                        Id = Convert.ToInt32(reader["id_compra"]),
                        IdUsuario = Convert.ToInt32(reader["id_usuario"]),
                        IdCarro = Convert.ToInt32(reader["id_carro"]),
                        NomeCliente = reader["nome_cliente"].ToString()!,
                        NomeCarro = reader["nome_carro"].ToString()!,
                        Categoria = reader["categoria"].ToString()!,
                        Imagem = reader["imagem"] == DBNull.Value
                            ? null
                            : reader["imagem"].ToString(),
                        Valor = Convert.ToDecimal(reader["valor"]),
                        DataCompra = Convert.ToDateTime(reader["data_compra"]),
                        DataRetirada = Convert.ToDateTime(reader["data_retirada"])
                    };  
                    
                }

                return null;
            }
        }
    }
}
