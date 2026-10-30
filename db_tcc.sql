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
    id_marca INT,
    id_categoria INT,
    descricao TEXT,
    ano INT NOT NULL DEFAULT 2026,
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

DELIMITER ;