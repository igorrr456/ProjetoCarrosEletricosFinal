using MySql.Data.MySqlClient;
using ProjetoCarros.Interfaces;
using ProjetoCarros.Models;
using BCrypt;

namespace ProjetoCarros.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly string _connectionString;

        public UsuarioRepositorio(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Conexao");
        }

        public Usuario Validar(string login, string senha)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand("sp_autenticar_usuario", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@p_login", login);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string senhaBanco = reader["senha"].ToString()!;
                if (BCrypt.Net.BCrypt.Verify(senha, senhaBanco))
                {
                    return new Usuario
                    {
                        Id = Convert.ToInt32(reader["id_usuario"]),
                        Nome = reader["nome"].ToString()!,
                        Email = reader["email"].ToString()!,
                        Nivel = reader["nivel"].ToString()!
                    };
                }
            }
            return null;
        }
        public void CriarConta(Usuario usuario)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string senhaHash = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);

            using var cmd = new MySqlCommand("sp_cadastrar_usuario", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_nome", usuario.Nome);
            cmd.Parameters.AddWithValue("@p_email", usuario.Email);
            cmd.Parameters.AddWithValue("@p_senha", senhaHash);
            cmd.Parameters.AddWithValue("@p_telefone", usuario.Telefone);

            cmd.ExecuteNonQuery();
        }

        //Metodo para fazer a exclusão de conta
        public void DeletarConta(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            using var cmd = new MySqlCommand("sp_desativar_usuario", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_id_usuario", id);

            cmd.ExecuteNonQuery();
        }

        //Metodo para buscar o usuario por Id
        public Usuario BuscarPorId(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            using var cmd = new MySqlCommand("sp_buscar_usuario", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_id_usuario", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new Usuario
                {
                    Id = Convert.ToInt32(reader["id_usuario"]),
                    Nome = reader["nome"].ToString()!,
                    Email = reader["email"].ToString()!,
                    Senha = reader["senha"].ToString()!,
                    Telefone = reader["telefone"] == DBNull.Value
                        ? null
                        : reader["telefone"].ToString(),
                    Nivel = reader["nivel"].ToString()!
                };
            }

            return null;
        }

        public void Atualizar(Usuario usuario, string? novaSenha = null)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            using var cmd = new MySqlCommand("sp_editar_usuario", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            string senhaHash = string.IsNullOrWhiteSpace(novaSenha)
                ? BuscarPorId(usuario.Id).Senha
                : BCrypt.Net.BCrypt.HashPassword(novaSenha);

            cmd.Parameters.AddWithValue("@p_id_usuario", usuario.Id);
            cmd.Parameters.AddWithValue("@p_nome", usuario.Nome);
            cmd.Parameters.AddWithValue("@p_email", usuario.Email);
            cmd.Parameters.AddWithValue("@p_senha", senhaHash);
            cmd.Parameters.AddWithValue("@p_telefone", usuario.Telefone);

            cmd.ExecuteNonQuery();
        }
    }
}