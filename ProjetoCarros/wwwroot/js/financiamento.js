document.addEventListener("DOMContentLoaded", function () {
    const selectCarro = document.getElementById("selectCarro");

    const carroPreview = document.getElementById("carroPreview");
    const previewImagem = document.getElementById("previewImagem");
    const previewNome = document.getElementById("previewNome");
    const previewPreco = document.getElementById("previewPreco");

    const financiamentoForm = document.getElementById("financiamentoForm");
    const rangeEntrada = document.getElementById("rangeEntrada");
    const rangeParcelas = document.getElementById("rangeParcelas");

    const valorEntradaLabel = document.getElementById("valorEntradaLabel");
    const percentEntradaLabel = document.getElementById("percentEntradaLabel");
    const valorParcelasLabel = document.getElementById("valorParcelasLabel");

    const resValorCarro = document.getElementById("resValorCarro");
    const resEntrada = document.getElementById("resEntrada");
    const resFinanciado = document.getElementById("resFinanciado");
    const resParcela = document.getElementById("resParcela");
    const resTotal = document.getElementById("resTotal");
    const resJuros = document.getElementById("resJuros");

    const TAXA_JUROS_MENSAL = 0.0199; // 1,99% a.m.

    function formatarMoeda(valor) {
        return valor.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
    }

    // Fórmula da Tabela Price (juros compostos / PMT)
    function calcularParcela(valorFinanciado, taxa, parcelas) {
        if (parcelas <= 0) return 0;
        if (taxa === 0) return valorFinanciado / parcelas;
        const fator = Math.pow(1 + taxa, -parcelas);
        return (valorFinanciado * taxa) / (1 - fator);
    }

    function atualizarSimulacao() {
        const option = selectCarro.options[selectCarro.selectedIndex];
        const preco = parseFloat(option.dataset.preco);

        if (!preco || isNaN(preco)) return;

        const percentEntrada = parseInt(rangeEntrada.value, 10);
        const parcelas = parseInt(rangeParcelas.value, 10);

        const valorEntrada = preco * (percentEntrada / 100);
        const valorFinanciado = preco - valorEntrada;

        const parcela = calcularParcela(valorFinanciado, TAXA_JUROS_MENSAL, parcelas);
        const totalPago = (parcela * parcelas) + valorEntrada;
        const totalJuros = totalPago - preco;

        percentEntradaLabel.textContent = percentEntrada;
        valorEntradaLabel.textContent = formatarMoeda(valorEntrada);
        valorParcelasLabel.textContent = parcelas + "x";

        resValorCarro.textContent = formatarMoeda(preco);
        resEntrada.textContent = formatarMoeda(valorEntrada);
        resFinanciado.textContent = formatarMoeda(valorFinanciado);
        resParcela.textContent = formatarMoeda(parcela) + " /mês";
        resTotal.textContent = formatarMoeda(totalPago);
        resJuros.textContent = formatarMoeda(totalJuros);
    }

    function carregarCarroSelecionado() {
        const option = selectCarro.options[selectCarro.selectedIndex];

        if (!option.value) {
            carroPreview.classList.add("d-none");
            financiamentoForm.classList.add("d-none");
            return;
        }

        const preco = parseFloat(option.dataset.preco);

        previewImagem.src = option.dataset.imagem || "/imagens/carros/sem-imagem.png";
        previewNome.textContent = option.dataset.marca + " " + option.dataset.nome;
        previewPreco.textContent = formatarMoeda(preco);

        carroPreview.classList.remove("d-none");
        financiamentoForm.classList.remove("d-none");

        atualizarSimulacao();
    }

    selectCarro.addEventListener("change", carregarCarroSelecionado);
    rangeEntrada.addEventListener("input", atualizarSimulacao);
    rangeParcelas.addEventListener("input", atualizarSimulacao);

    // Se a página já veio com um carro pré-selecionado (?id=5), carrega direto
    if (selectCarro.value) {
        carregarCarroSelecionado();
    }
});
