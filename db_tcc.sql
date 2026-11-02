/*	NÃO EXECUTAR SEM AUTORIZAÇÃO
	DROP DATABASE IF EXISTS db_tcc; */

CREATE DATABASE db_tcc;

USE db_tcc;

CREATE TABLE tb_usuario (
	id_usuario INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    senha VARCHAR(150) NOT NULL,
    telefone VARCHAR(20),
    nivel ENUM('Usuario', 'Admin') DEFAULT 'Usuario',
    ativo BOOL DEFAULT TRUE,
    deletado_em DATETIME DEFAULT NULL
);

CREATE TABLE tb_funcionario (
	id_usuario INT PRIMARY KEY,
    cpf VARCHAR(14) NOT NULL UNIQUE,
    data_contratacao DATETIME NOT NULL,
    FOREIGN KEY (id_usuario) REFERENCES tb_usuario(id_usuario)
);

CREATE TABLE tb_marca (
	id_marca INT PRIMARY KEY AUTO_INCREMENT,
    nome_marca VARCHAR(50) NOT NULL UNIQUE,
    ativa BOOL DEFAULT TRUE,
    deletada_em DATETIME DEFAULT NULL
);

CREATE TABLE tb_categoria(
	id_categoria INT PRIMARY KEY AUTO_INCREMENT,
    nome_categoria VARCHAR(20) NOT NULL UNIQUE,
    hibrido BOOL NOT NULL DEFAULT FALSE,
    ativa BOOL DEFAULT TRUE,
    deletada_em DATETIME DEFAULT NULL
);

CREATE TABLE tb_carro (
	id_carro INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(100) NOT NULL,
    preco DECIMAL(10,2) NOT NULL,
    id_marca INT NOT NULL,
    id_categoria INT NOT NULL,
    descricao TEXT,
    ano INT NOT NULL DEFAULT (YEAR(CURRENT_DATE)),
    data_cadastro DATETIME DEFAULT CURRENT_TIMESTAMP,
    imagem VARCHAR(250),
    disponivel BOOL DEFAULT TRUE,
    desativado_em DATETIME DEFAULT NULL,
    FOREIGN KEY (id_marca) REFERENCES tb_marca(id_marca),
    FOREIGN KEY (id_categoria) REFERENCES tb_categoria(id_categoria)
);

CREATE TABLE tb_compra (
	id_compra INT PRIMARY KEY AUTO_INCREMENT,
    id_usuario INT NOT NULL,
    id_carro INT NOT NULL,
    valor DECIMAL(10,2) NOT NULL,
    data_compra DATETIME DEFAULT CURRENT_TIMESTAMP,
    data_retirada DATE NOT NULL,
    status ENUM('Concluido', 'Cancelado', 'Estornado') DEFAULT 'Concluido',
    FOREIGN KEY (id_usuario) REFERENCES tb_usuario(id_usuario),
    FOREIGN KEY (id_carro) REFERENCES tb_carro(id_carro)
);

/*
	INÍCIO DAS PROCEDURES
*/

DELIMITER $$

/*
	PROCEDURES USUÁRIOS
*/

-- CADASTRAR USUÁRIO COMUM
CREATE PROCEDURE sp_cadastrar_usuario(
	IN p_nome VARCHAR(100),
    IN p_email VARCHAR(150),
    IN p_senha VARCHAR(150),
    IN p_telefone VARCHAR(20)
)
BEGIN
	INSERT INTO tb_usuario (nome, email, senha, telefone)
    VALUES (p_nome, p_email, p_senha, p_telefone);
END $$

-- CADASTRAR FUNCIONÁRIO
CREATE PROCEDURE sp_cadastrar_funcionario (
    IN p_nome VARCHAR(100),
    IN p_email VARCHAR(150),
    IN p_senha VARCHAR(150),
    IN p_telefone VARCHAR(20),
    IN p_cpf VARCHAR(14),
    IN p_data_contratacao DATETIME
)
BEGIN
    DECLARE v_id_usuario INT;

	-- DECLARE EXIT HANDLER: ENCERRA A EXECUÇÃO DEPOIS DE REALIZAR OS PROCEDIMENTOS
    -- SQLEXCEPTION: TIPO DE ERRO, USADO PARA ERROS SQL
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
		-- DESFAZ AS ALTERAÇÕES
        ROLLBACK;
        -- ENVIA NOVAMENTE O ERRO
        RESIGNAL;
    END;

	-- CONJUNTO DE OPERAÇÕES
    START TRANSACTION;

    INSERT INTO tb_usuario (nome, email, senha, telefone, nivel)
    VALUES (p_nome, p_email, p_senha, p_telefone, 'Admin');

	-- ARMAZENANDO O ID PARA USAR NO CADASTRO DE FUNCIONÁRIO
    SET v_id_usuario = LAST_INSERT_ID();

    INSERT INTO tb_funcionario(id_usuario, cpf, data_contratacao)
    VALUES (v_id_usuario, p_cpf, p_data_contratacao);

	-- FINALIZANDO TRANSAÇÃO
    COMMIT;
END $$

CREATE PROCEDURE sp_buscar_usuario(
	IN p_id_usuario INT
)
BEGIN
	SELECT 
		id_usuario,
        nome,
        email,
        senha,
        telefone,
        nivel
	FROM tb_usuario
    WHERE id_usuario = p_id_usuario
		AND ativo = TRUE;
END $$

-- EDITAR USUÁRIO (ATIVO)
CREATE PROCEDURE sp_editar_usuario(
	IN p_id_usuario INT,
    IN p_nome VARCHAR(100),
    IN p_email VARCHAR(150),
    IN p_senha VARCHAR(150),
    IN p_telefone VARCHAR(20)
)
BEGIN
	UPDATE tb_usuario
    SET
		nome = p_nome,
        email = p_email,
        senha = p_senha,
        telefone = p_telefone
	WHERE id_usuario = p_id_usuario
    AND ativo = TRUE;
END $$

-- APAGAR USUÁRIO (SOFT DELETE)
CREATE PROCEDURE sp_desativar_usuario(
    IN p_id_usuario INT
)
BEGIN
    UPDATE tb_usuario 
    SET ativo = FALSE, 
        deletado_em = NOW() 
    WHERE id_usuario = p_id_usuario;
END $$

-- LOGIN DO USUÁRIO (VIA EMAIL (FUNCIONÁRIO) OU VIA CPF(FUNCIONÁRIO))
CREATE PROCEDURE sp_autenticar_usuario(
    IN p_login VARCHAR(150)
)
BEGIN
    SELECT
        u.id_usuario,
        u.nome,
        u.email,
        u.senha,
        u.nivel,
        f.cpf
    FROM tb_usuario u
    LEFT JOIN tb_funcionario f
        ON u.id_usuario = f.id_usuario
    WHERE (u.email = p_login OR f.cpf = p_login)
      AND u.ativo = TRUE;
END $$

/*
	PROCEDURES CATEGORIAS
*/

-- CADASTRAR CATEGORIA
CREATE PROCEDURE sp_cadastrar_categoria(
    IN p_nome_categoria VARCHAR(20)
)
BEGIN
    INSERT INTO tb_categoria (nome_categoria) 
    VALUES (p_nome_categoria);
END $$

-- MOSTRAR MARCAS (ATIVAS)
CREATE PROCEDURE sp_listar_categorias()
BEGIN
    SELECT id_categoria AS numero_categoria, 
		   nome_categoria AS categoria
    FROM tb_categoria
    WHERE ativa = TRUE;
END $$

-- EDITAR CATEGORIA (ATIVA)
CREATE PROCEDURE sp_editar_categoria(
    IN p_id_categoria INT,
    IN p_nome_categoria VARCHAR(20)
)
BEGIN
    UPDATE tb_categoria
    SET nome_categoria = p_nome_categoria
    WHERE id_categoria = p_id_categoria
    AND ativa = TRUE;
END $$

-- APAGAR CATEGORIA (SOFT DELETE)
CREATE PROCEDURE sp_desativar_categoria(
    IN p_id_categoria INT
)
BEGIN
    UPDATE tb_categoria
    SET ativa = FALSE,
        deletada_em = NOW()
    WHERE id_categoria = p_id_categoria;
END $$

/* 
	PROCEDURES MARCAS
*/

-- CADASTRAR MARCA
CREATE PROCEDURE sp_cadastrar_marca(
    IN p_nome_marca VARCHAR(50)
)
BEGIN
    INSERT INTO tb_marca (nome_marca) 
    VALUES (p_nome_marca);
END $$

-- MOSTRAR MARCAS (ATIVAS)
CREATE PROCEDURE sp_listar_marcas()
BEGIN
    SELECT id_marca AS 'Número da Marca', 
		   nome_marca AS Marca
    FROM tb_marca 
    WHERE ativa = TRUE;
END $$

-- EDITAR MARCA (ATIVA)
CREATE PROCEDURE sp_editar_marca(
    IN p_id_marca INT,
    IN p_nome_marca VARCHAR(50)
)
BEGIN
    UPDATE tb_marca
    SET nome_marca = p_nome_marca
    WHERE id_marca = p_id_marca 
    AND ativa = TRUE;
END $$

-- APAGAR MARCA (SOFT DELETE)
CREATE PROCEDURE sp_desativar_marca(
    IN p_id_marca INT
)
BEGIN
    UPDATE tb_marca
    SET ativa = FALSE,
        deletada_em = NOW()
    WHERE id_marca = p_id_marca;
END $$

/*
	PROCEDURES CARROS
*/

-- CADASTRAR CARRO
CREATE PROCEDURE sp_cadastrar_carro(
    IN p_nome VARCHAR(100),
    IN p_preco DECIMAL(10,2),
    IN p_id_categoria INT,
    IN p_id_marca INT,
    IN p_ano INT,
    IN p_descricao TEXT,
    IN p_imagem VARCHAR(250)
)
BEGIN
    INSERT INTO tb_carro (
        nome,
        preco,
        id_categoria,
        id_marca,
        descricao,
        ano,
        imagem
    )
    VALUES (
        p_nome,
        p_preco,
        p_id_categoria,
        p_id_marca,
        p_descricao,
        p_ano,
        p_imagem
    );
END $$

CREATE PROCEDURE sp_listar_carros()
BEGIN
	SELECT
    c.id_carro,
    c.nome,
    c.preco,
    c.id_marca,
    c.id_categoria,
    cat.nome_categoria AS categoria,
    m.nome_marca AS marca,
    c.descricao,
    c.ano,
    c.data_cadastro,
    c.imagem,
    c.disponivel
	FROM tb_carro c
	INNER JOIN tb_marca m
		ON c.id_marca = m.id_marca
	INNER JOIN tb_categoria cat
		ON c.id_categoria = cat.id_categoria
    WHERE c.desativado_em IS NULL
    AND c.disponivel = TRUE
    ORDER BY c.data_cadastro DESC;
END $$

CREATE PROCEDURE sp_listar_hibridos()
BEGIN
    SELECT
    c.id_carro,
    c.nome,
    c.preco,
    c.id_marca,
    c.id_categoria,
    cat.nome_categoria AS categoria,
    m.nome_marca AS marca,
    c.descricao,
    c.ano,
    c.data_cadastro,
    c.imagem,
    c.disponivel
	FROM tb_carro c
	INNER JOIN tb_marca m
		ON c.id_marca = m.id_marca
	INNER JOIN tb_categoria cat
		ON c.id_categoria = cat.id_categoria
    WHERE cat.hibrido = TRUE
      AND c.disponivel = TRUE
      AND c.desativado_em IS NULL
    ORDER BY c.data_cadastro DESC;
END $$

CREATE PROCEDURE sp_listar_eletricos()
BEGIN
    SELECT
    c.id_carro,
    c.nome,
    c.preco,
    c.id_marca,
    c.id_categoria,
    cat.nome_categoria AS categoria,
    m.nome_marca AS marca,
    c.descricao,
    c.ano,
    c.data_cadastro,
    c.imagem,
    c.disponivel
	FROM tb_carro c
	INNER JOIN tb_marca m
		ON c.id_marca = m.id_marca
	INNER JOIN tb_categoria cat
		ON c.id_categoria = cat.id_categoria
    WHERE cat.hibrido = FALSE
      AND c.disponivel = TRUE
      AND c.desativado_em IS NULL
    ORDER BY c.data_cadastro DESC;
END $$

CREATE PROCEDURE sp_buscar_carro(
    IN p_id_carro INT
)
BEGIN
    SELECT
        c.id_carro,
        c.nome,
        c.preco,
        c.id_categoria,
        cat.nome_categoria AS categoria,
        c.id_marca,
        m.nome_marca AS marca,
        c.descricao,
        c.ano,
        c.data_cadastro,
        c.imagem,
        c.disponivel
    FROM tb_carro c
    INNER JOIN tb_marca m
        ON c.id_marca = m.id_marca
    INNER JOIN tb_categoria cat
        ON c.id_categoria = cat.id_categoria
    WHERE c.id_carro = p_id_carro
      AND c.disponivel = TRUE
      AND c.desativado_em IS NULL;
END $$

-- EDITAR CARRO (ATIVO)
CREATE PROCEDURE sp_editar_carro(
    IN p_id_carro INT,
    IN p_nome VARCHAR(100),
    IN p_preco DECIMAL(10,2),
    IN p_id_categoria INT,
    IN p_id_marca INT,
    IN p_descricao TEXT,
    IN p_ano INT,
    IN p_imagem VARCHAR(250)
)
BEGIN
    IF EXISTS (
        SELECT 1
        FROM tb_carro
        WHERE id_carro = p_id_carro
          AND disponivel = TRUE
          AND desativado_em IS NULL
    ) THEN
    
        UPDATE tb_carro
        SET
            nome = p_nome,
            preco = p_preco,
            id_categoria = p_id_categoria,
            id_marca = p_id_marca,
            descricao = p_descricao,
            ano = p_ano,
            imagem = p_imagem
        WHERE id_carro = p_id_carro
          AND disponivel = TRUE
          AND desativado_em IS NULL;

        SELECT TRUE AS atualizado;
    ELSE
        SELECT FALSE AS atualizado;
    END IF;
END $$

-- APAGAR CARRO (SOFT DELETE)
CREATE PROCEDURE sp_desativar_carro(
    IN p_id_carro INT
)
BEGIN
    UPDATE tb_carro
    SET disponivel = FALSE,
        desativado_em = NOW()
    WHERE id_carro = p_id_carro;
END $$

/*
	PROCEDURES COMPRAS
*/

-- CADASTRAR COMPRA
CREATE PROCEDURE sp_cadastrar_compra (
	IN p_id_usuario INT,
    IN p_id_carro INT,
    IN p_valor DECIMAL(10,2),
    IN p_data_retirada DATE
)
BEGIN
	INSERT INTO tb_compra(id_usuario, id_carro, valor, data_retirada)
    VALUES (p_id_usuario, p_id_carro, p_valor, p_data_retirada);
END $$

-- BUSCAR POR ID
CREATE PROCEDURE sp_buscar_compra(
	IN p_id_compra INT
)
BEGIN
	SELECT
		cp.id_compra,
        cp.id_usuario,
        cp.id_carro,
        cp.valor,
        cp.data_compra,
        cp.data_retirada,
        c.nome AS nome_carro,
        c.imagem,
        cat.nome_categoria AS categoria,
        u.nome AS nome_cliente
	FROM tb_compra cp
    INNER JOIN tb_carro c
		ON cp.id_carro = c.id_carro
	INNER JOIN tb_categoria cat
		ON c.id_categoria = cat.id_categoria
	INNER JOIN tb_usuario u
		ON cp.id_usuario = u.id_usuario
	WHERE cp.id_compra = p_id_compra;
END $$

-- LISTAR COMPRAS POR USUÁRIO
CREATE PROCEDURE sp_listar_compras_usuario (
	IN p_id_usuario INT
)
BEGIN
	SELECT cp.id_compra AS 'Número da Compra',
		   u.nome AS 'Nome do Comprador',
		   ca.nome AS Carro,
           m.nome_marca AS Marca,
		   cp.valor AS 'Valor da Compra',
		   cp.data_compra AS 'Data da Compra',
		   cp.data_retirada AS 'Data da Retirada',
           cp.status AS 'Status'
    FROM tb_compra cp
    INNER JOIN tb_usuario u ON c.id_usuario = u.id_usuario
    INNER JOIN tb_carro ca ON c.id_carro = ca.id_carro
    INNER JOIN tb_marca m ON ca.id_marca = m.id_marca 
    WHERE c.id_usuario = p_id_usuario;
END $$

-- LISTAR TODAS AS COMPRAS
CREATE PROCEDURE sp_listar_compras ()
BEGIN
	SELECT cp.id_compra AS 'Número da Compra',
		   u.nome AS 'Nome do Comprador',
		   ca.nome AS Carro,
           m.nome_marca AS Marca,
		   cp.valor AS 'Valor da Compra',
		   cp.data_compra AS 'Data da Compra',
		   cp.data_retirada AS 'Data da Retirada',
           cp.status AS 'Status'
    FROM tb_compra cp
    INNER JOIN tb_usuario u ON c.id_usuario = u.id_usuario
    INNER JOIN tb_carro ca ON c.id_carro = ca.id_carro
    INNER JOIN tb_marca m ON ca.id_marca = m.id_marca;
END $$

-- ATUALIZAR ESTADO DA COMPRA
CREATE PROCEDURE sp_atualizar_status_compra(
    IN p_id_compra INT,
    IN p_status VARCHAR(20)
)
BEGIN
    UPDATE tb_compra
    SET status = p_status
    WHERE id_compra = p_id_compra;
END $$

DELIMITER ;
