document.addEventListener("DOMContentLoaded", function () {
    var mapaEl = document.getElementById("mapaCarregamento");
    if (!mapaEl) return;


    var map = L.map("mapaCarregamento").setView([-23.55, -46.63], 6);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "&copy; OpenStreetMap contributors",
        maxZoom: 19
    }).addTo(map);


    var pontosDeCarregamento = [
        { nome: "ETEC Motors", lat: -23.520790898757777, lng: -46.72778482019172 }

    ];

    pontosDeCarregamento.forEach(function (ponto) {
        L.marker([ponto.lat, ponto.lng])
            .addTo(map)
            .bindPopup("<strong>" + ponto.nome + "</strong>");
    });
});
