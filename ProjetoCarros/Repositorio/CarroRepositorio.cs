using MySql.Data.MySqlClient;
using ProjetoCarros.Interfaces;
using ProjetoCarros.Models;

namespace ProjetoCarros.Repositorio
{
    public class CarroRepositorio : ICarroRepositorio
    {
        private readonly string _connectionString;
        //Serve para nos dar acesso ao wwwroot para salvar as imagens
        private readonly IWebHostEnvironment _env;

        public CarroRepositorio(IConfiguration config, IWebHostEnvironment env)
        {
            _connectionString = config.GetConnectionString("Conexao");
            _env = env;
        }

        //Parte da listagem de carros

        public List<Carro> ListarTodos()
        {
            return ExecutarListagem("sp_listar_carros");
        }

        public List<Carro> ListarHibridos()
        {
            return ExecutarListagem("sp_listar_hibridos");
        }

        public List<Carro> ListarEletricos()
        {
            return ExecutarListagem("sp_listar_eletricos");
        }

        private List<Carro> ExecutarListagem(string procedure)
        {
            var lista = new List<Carro>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            using var cmd = new MySqlCommand(procedure, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(MapearCarro(reader));
            }

            return lista;
        }

        //Buscar um carro so pelo id
        public Carro? BuscarPorId(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            using var cmd = new MySqlCommand("sp_buscar_carro", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_id_carro", id);

            //A diferença é que com using (var reader = ...)
            //com chaves, o reader é fechado e liberado no } final,
            //antes do método retornar. Sem as chaves, o using var só libera quando sai do escopo do método inteiro,
            //o que causa conflito quando o Deletar tenta abrir outra conexão logo depois.
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                    return MapearCarro(reader);

                return null;
            }
        }

        //Criaçao de um carro
        public void Criar(Carro carro, IFormFile? imagem)
        {
            string? imagemSalva = null;

            try
            {
                // Salva a imagem, se uma foi enviada
                imagemSalva = SalvarImagem(imagem);
                carro.Imagem = imagemSalva;

                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                using var cmd = new MySqlCommand("sp_cadastrar_carro", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@p_nome", carro.Nome);
                cmd.Parameters.AddWithValue("@p_preco", carro.Preco);
                cmd.Parameters.AddWithValue("@p_id_categoria", carro.IdCategoria);
                cmd.Parameters.AddWithValue("@p_id_marca", carro.IdMarca);
                cmd.Parameters.AddWithValue("@p_ano", carro.Ano);
                cmd.Parameters.AddWithValue(
                    "@p_descricao",
                    (object?)carro.Descricao ?? DBNull.Value
                );
                cmd.Parameters.AddWithValue(
                    "@p_imagem",
                    (object?)carro.Imagem ?? DBNull.Value
                );

                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Se o cadastro falhar, remove a imagem que acabou de ser salva
                if (imagemSalva != null)
                    DeletarImagemAntiga(imagemSalva);

                throw;
            }
        }

        // Update dos carros existentes
        public void Editar(Carro carro, IFormFile? imagem)
        {
            string? imagemAntiga = carro.Imagem;
            string? imagemNova = null;

            bool trocouImagem = imagem != null && imagem.Length > 0;
            bool atualizado = false;

            try
            {
                // Salva a nova imagem, se uma foi enviada
                if (trocouImagem)
                {
                    imagemNova = SalvarImagem(imagem);
                    carro.Imagem = imagemNova;
                }

                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                using var cmd = new MySqlCommand("sp_editar_carro", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@p_id_carro", carro.Id);
                cmd.Parameters.AddWithValue("@p_nome", carro.Nome);
                cmd.Parameters.AddWithValue("@p_preco", carro.Preco);
                cmd.Parameters.AddWithValue("@p_id_categoria", carro.IdCategoria);
                cmd.Parameters.AddWithValue("@p_id_marca", carro.IdMarca);
                cmd.Parameters.AddWithValue(
                    "@p_descricao",
                    (object?)carro.Descricao ?? DBNull.Value
                );
                cmd.Parameters.AddWithValue("@p_ano", carro.Ano);
                cmd.Parameters.AddWithValue(
                    "@p_imagem",
                    (object?)carro.Imagem ?? DBNull.Value
                );

                // Lê o resultado retornado pela procedure
                object? resultado = cmd.ExecuteScalar();

                atualizado = resultado != null
                    && resultado != DBNull.Value
                    && Convert.ToBoolean(resultado);

                if (!atualizado)
                {
                    throw new InvalidOperationException(
                        "O carro não foi atualizado. Ele pode não existir ou estar desativado."
                    );
                }
            }
            catch
            {
                // Se a atualização falhar, remove somente a imagem nova
                if (imagemNova != null)
                    DeletarImagemAntiga(imagemNova);

                throw;
            }

            // Só remove a imagem antiga após confirmar a atualização
            if (atualizado && trocouImagem && imagemAntiga != null)
                DeletarImagemAntiga(imagemAntiga);
        }

        //exclusao do card de carros

        public void Deletar(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            using var cmd = new MySqlCommand("sp_desativar_carro", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_id_carro", id);
            cmd.ExecuteNonQuery();
        }

        //Metodos privados para auxiliação

        //Evita a repetição do mapeamento reader
        private Carro MapearCarro(MySqlDataReader reader)
        {
            return new Carro
            {
                Id = Convert.ToInt32(reader["id_carro"]),
                Nome = reader["nome"].ToString()!,

                IdMarca = Convert.ToInt32(reader["id_marca"]),
                Marca = reader["marca"].ToString(),

                IdCategoria = Convert.ToInt32(reader["id_categoria"]),
                Categoria = reader["categoria"].ToString(),

                Descricao = reader["descricao"] == DBNull.Value
                    ? null
                    : reader["descricao"].ToString(),

                Ano = Convert.ToInt32(reader["ano"]),

                Imagem = reader["imagem"] == DBNull.Value
                    ? null
                    : reader["imagem"].ToString(),

                Preco = Convert.ToDecimal(reader["preco"]),
                DataCadastro = Convert.ToDateTime(reader["data_cadastro"])
            };
        }

        private string? SalvarImagem(IFormFile? imagem)
        {
            if (imagem == null || imagem.Length == 0) return null;

            //Gera um nome unico para não sobrescrever imagens com o mesmo nome
            string nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(imagem.FileName);
            string pasta = Path.Combine(_env.WebRootPath, "imagens", "carros");

            //Criar a pasta caso não exista

            Directory.CreateDirectory(pasta);

            string caminhoCompleto = Path.Combine(pasta, nomeArquivo);
            using var stream = new FileStream(caminhoCompleto, FileMode.Create);
            imagem.CopyTo(stream);

            // Retorna o caminho relativo para salvar no bannco e usar no img src 

            return "/imagens/carros/" + nomeArquivo;
        }

        private void DeletarImagemAntiga(string? caminhoImagem)
        {
            if (string.IsNullOrEmpty(caminhoImagem)) return;

            //Converte o caminho relativo para caminho fisico no servidor 
            string caminhoFisico = Path.Combine(_env.WebRootPath, caminhoImagem.TrimStart('/'));
            if (File.Exists(caminhoFisico))
                File.Delete(caminhoFisico);
        }
    }
}
