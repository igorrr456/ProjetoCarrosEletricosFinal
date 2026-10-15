(function () {
    "use strict";

    var diasSemana = ["Domingo", "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira", "Sábado"];

    var horariosDisponiveis = [
        "08:00", "08:30", "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
        "13:30", "14:00", "14:30", "15:00", "15:30"
    ];

    var estado = {
        dataSelecionada: null,   // objeto Date
        horarioSelecionado: null // string "08:00"
    };

    var datesScroll = document.getElementById("datesScroll");
    var horariosGrid = document.getElementById("horariosGrid");
    var btnStep1Next = document.getElementById("btnStep1Next");

    function pad(n) { return n < 10 ? "0" + n : "" + n; }

    function formatarData(date) {
        return pad(date.getDate()) + "/" + pad(date.getMonth() + 1) + "/" + date.getFullYear();
    }

    function gerarDatas() {
        datesScroll.innerHTML = "";
        var hoje = new Date();

        for (var i = 0; i < 14; i++) {
            var d = new Date(hoje);
            d.setDate(hoje.getDate() + i);

            var card = document.createElement("div");
            card.className = "ag-date-card";
            card.dataset.iso = d.toISOString();

            card.innerHTML =
                '<div class="ag-date-dia">' + diasSemana[d.getDay()] + '</div>' +
                '<div class="ag-date-data">' + formatarData(d) + '</div>';

            card.addEventListener("click", function () {
                document.querySelectorAll(".ag-date-card").forEach(function (c) { c.classList.remove("selected"); });
                this.classList.add("selected");
                estado.dataSelecionada = new Date(this.dataset.iso);
                estado.horarioSelecionado = null;
                document.querySelectorAll(".ag-horario-btn").forEach(function (b) { b.classList.remove("selected"); });
                checarHabilitarProximo();
            });

            datesScroll.appendChild(card);
        }
    }

    function gerarHorarios() {
        horariosGrid.innerHTML = "";
        horariosDisponiveis.forEach(function (h) {
            var btn = document.createElement("button");
            btn.type = "button";
            btn.className = "ag-horario-btn";
            btn.textContent = h;

            btn.addEventListener("click", function () {
                document.querySelectorAll(".ag-horario-btn").forEach(function (b) { b.classList.remove("selected"); });
                this.classList.add("selected");
                estado.horarioSelecionado = h;
                checarHabilitarProximo();
            });

            horariosGrid.appendChild(btn);
        });
    }

    function checarHabilitarProximo() {
        btnStep1Next.disabled = !(estado.dataSelecionada && estado.horarioSelecionado);
    }

    function setStepVisual(passo) {
        for (var i = 1; i <= 3; i++) {
            var circle = document.getElementById("circle-" + i);
            circle.classList.remove("active", "done");
            if (i < passo) circle.classList.add("done");
            if (i === passo) circle.classList.add("active");
        }
        var line1 = document.getElementById("line-1");
        var line2 = document.getElementById("line-2");
        line1.classList.toggle("done", passo > 1);
        line2.classList.toggle("done", passo > 2);
    }

    function irParaPasso(passo) {
        document.getElementById("panel-1").classList.toggle("d-none", passo !== 1);
        document.getElementById("panel-2").classList.toggle("d-none", passo !== 2);
        document.getElementById("panel-3").classList.toggle("d-none", passo !== 3);
        setStepVisual(passo);
    }

    function gerarProtocolo() {
        return Math.floor(100000 + Math.random() * 900000);
    }

    // ---- eventos de navegação ----
    document.getElementById("btnStep1Next").addEventListener("click", function () {
        document.getElementById("resumoData").textContent =
            diasSemana[estado.dataSelecionada.getDay()] + ", " + formatarData(estado.dataSelecionada);
        document.getElementById("resumoHorario").textContent = estado.horarioSelecionado;
        irParaPasso(2);
    });

    document.getElementById("btnStep2Back").addEventListener("click", function () {
        irParaPasso(1);
    });

    document.getElementById("loginForm").addEventListener("submit", function (e) {
        e.preventDefault();

        var email = document.getElementById("loginEmail").value.trim();
        var senha = document.getElementById("loginSenha").value.trim();
        var erroEl = document.getElementById("loginErro");
        var btnSubmit = this.querySelector('button[type="submit"]');

        if (!email || !senha) return;

        erroEl.classList.add("d-none");
        erroEl.textContent = "";
        btnSubmit.disabled = true;
        var textoOriginal = btnSubmit.textContent;
        btnSubmit.textContent = "Entrando...";

        fetch("/Usuario/LogarAjax", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email: email, senha: senha })
        })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                btnSubmit.disabled = false;
                btnSubmit.textContent = textoOriginal;

                if (!data.sucesso) {
                    erroEl.textContent = data.mensagem;
                    erroEl.classList.remove("d-none");
                    return;
                }

                document.getElementById("confProtocolo").textContent = gerarProtocolo();
                document.getElementById("confData").textContent =
                    diasSemana[estado.dataSelecionada.getDay()] + ", " + formatarData(estado.dataSelecionada);
                document.getElementById("confHorario").textContent = estado.horarioSelecionado;
                document.getElementById("confCliente").textContent = data.nome;

                irParaPasso(3);
            })
            .catch(function () {
                btnSubmit.disabled = false;
                btnSubmit.textContent = textoOriginal;
                erroEl.textContent = "Erro ao conectar com o servidor. Tente novamente.";
                erroEl.classList.remove("d-none");
            });
    });

    document.getElementById("btnNovoAgendamento").addEventListener("click", function () {
        estado.dataSelecionada = null;
        estado.horarioSelecionado = null;
        document.getElementById("loginForm").reset();
        gerarDatas();
        gerarHorarios();
        checarHabilitarProximo();
        irParaPasso(1);
    });

    // ---- inicialização ----
    gerarDatas();
    gerarHorarios();
})();
