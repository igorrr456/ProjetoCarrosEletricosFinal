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

        public Usuario Validar(string email, string senha)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT * FROM tb_usuario WHERE Email = @email";
            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string senhaBanco = reader["Senha"].ToString()!;
                if (BCrypt.Net.BCrypt.Verify(senha, senhaBanco))
                {
                    return new Usuario
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nome = reader["Nome"].ToString()!,
                        Email = reader["Email"].ToString()!,
                        Nivel = reader["Nivel"].ToString()!
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
            var sql = "INSERT INTO tb_usuario(Nome, Email, Senha, Nivel) VALUES (@nome, @email, @senha, @nivel)";
            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", usuario.Nome);
            cmd.Parameters.AddWithValue("@email", usuario.Email);
            cmd.Parameters.AddWithValue("@senha", senhaHash);
            cmd.Parameters.AddWithValue("@nivel", "Usuario");
            cmd.ExecuteNonQuery();
        }


        //Metodo para fazer a exclusão de conta

        public void DeletarConta(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = "DELETE FROM tb_usuario WHERE Id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }
        //Metodo para buscar o usuario por Id
        public Usuario BuscarPorId(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT *FROM tb_usuario WHERE Id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if(reader.Read())
            {
                return new Usuario
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nome = reader["Nome"].ToString()!,
                    Email = reader["Email"].ToString()!,
                    Senha = reader["Senha"].ToString()!,
                    Nivel = reader["Nivel"].ToString()!,
                };
            }

            return null;
        }

        public void Atualizar(Usuario usuario)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();


            var sql = @"UPDATE tb_usuario SET Nome = @nome, Email = @email WHERE Id = @id";
            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@nome", usuario.Nome);
            cmd.Parameters.AddWithValue("@email", usuario.Email);
            cmd.Parameters.AddWithValue("@id", usuario.Id);

            cmd.ExecuteNonQuery();
        }

    }
}