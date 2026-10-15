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

        public List<Carro>ListarTodos()
        {
            var lista = new List<Carro>();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT *FROM tb_carros ORDER BY Data_cadastro DESC";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read()) // vai retornar todos os carros existentes com o while
            {
                lista.Add(MapearCarro(reader));
            }
            return lista;
        }

       //Buscar um carro so pelo id
        public Carro BuscarPorId(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT * FROM tb_carros WHERE Id_carro = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            //A diferença é que com using (var reader = ...)
            //com chaves, o reader é fechado e liberado no } final,
            //antes do método retornar. Sem as chaves, o using var só libera quando sai do escopo do método inteiro,
            //o que causa conflito quando o Deletar tenta abrir outra conexão logo depois.
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read()) return MapearCarro(reader);
                return null;
            }

            
        }

        //Criaçao de um carro
        public void Criar (Carro carro, IFormFile? imagem)
        {
            // salva a imagem na pasta carros que esta dentro do wwwroot
            carro.Imagem = SalvarImagem(imagem);

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = @"INSERT INTO tb_carros(Nome_carro,Marca,Descricao,Imagem, Categoria, Preco) VALUES (@nome, @marca, @descricao, @imagem, @categoria, @preco)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", carro.Nome);
            cmd.Parameters.AddWithValue("@marca", carro.Marca);
            cmd.Parameters.AddWithValue("@descricao", carro.Descricao);
            cmd.Parameters.AddWithValue("@imagem", carro.Imagem);
            cmd.Parameters.AddWithValue("@categoria", carro.Categoria);
            cmd.Parameters.AddWithValue("@preco", carro.Preco);
            cmd.ExecuteNonQuery();
        }

        //Update dos carros já existentes
        public void Editar (Carro carro, IFormFile? imagem)
        {
            // So vai trocar de imagem se o adm enviou uma nova
            if(imagem!=null && imagem.Length>0)
            {
                DeletarImagemAntiga(carro.Imagem);//faz com que a imagem antiga seja apagada do servidor 
                carro.Imagem = SalvarImagem(imagem);
            }

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = @"UPDATE tb_carros SET
                            Nome_carro  = @nome,
                            Marca       = @marca,
                            Descricao   = @descricao,
                            Imagem      = @imagem,
                            Categoria   = @categoria,
                            Preco       = @preco
                        WHERE Id_carro = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", carro.Nome);
            cmd.Parameters.AddWithValue("@marca", carro.Marca);
            cmd.Parameters.AddWithValue("@descricao", carro.Descricao);
            cmd.Parameters.AddWithValue("@imagem", carro.Imagem);
            cmd.Parameters.AddWithValue("@categoria", carro.Categoria);
            cmd.Parameters.AddWithValue("@preco", carro.Preco);
            cmd.Parameters.AddWithValue("@id", carro.Id);
            cmd.ExecuteNonQuery();
        }

        //exclusao do card de carros

        public void Deletar(int id)
        {
            //vai buscar o carro pelo id antes de deletar para poder apagar a imagem do servidor
            var carro = BuscarPorId(id);
            if (carro != null) DeletarImagemAntiga(carro.Imagem);
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = "DELETE FROM tb_carros WHERE Id_carro = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        //Metodos privados para auxiliação

        //Evita a repetição do mapeamento reader
        private Carro MapearCarro(MySqlDataReader reader)
        {
            return new Carro
            {
                Id = Convert.ToInt32(reader["Id_carro"]),
                Nome = reader["Nome_carro"].ToString()!,
                Marca = reader["Marca"].ToString()!,
                Descricao = reader["Descricao"].ToString()!,
                Imagem = reader["Imagem"].ToString()!,
                Categoria = reader["Categoria"].ToString()!,
                Preco = Convert.ToDecimal(reader["Preco"]),
                DataCadastro = Convert.ToDateTime(reader["Data_cadastro"])
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
