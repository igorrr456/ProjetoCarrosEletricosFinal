document.addEventListener('DOMContentLoaded', function () {
    const carouselEl = document.getElementById('carouselHome');
    const video = document.getElementById('carouselVideo');

    if (!carouselEl || !video) return;

    // Inicia o carousel sem autoplay — o vídeo controla o primeiro slide
    const carousel = new bootstrap.Carousel(carouselEl, {
        ride: false,
        interval: false
    });

    // Toca o vídeo assim que a página carrega
    video.play();

    // Quando o vídeo terminar, avança para o próximo slide e liga o autoplay das imagens
    video.addEventListener('ended', function () {
        carousel.next();

        // Após sair do slide do vídeo, ativa o autoplay normal para os slides de imagem
        carouselEl._config = carouselEl._config || {};
        const bsCarousel = bootstrap.Carousel.getInstance(carouselEl);
        bsCarousel._config.interval = 5000;
        bsCarousel._config.ride = 'carousel';
        bsCarousel.cycle();
    });

    // Se o usuário voltar ao slide 0 manualmente, reinicia o vídeo e para o autoplay
    carouselEl.addEventListener('slid.bs.carousel', function (e) {
        if (e.to === 0) {
            const bsCarousel = bootstrap.Carousel.getInstance(carouselEl);
            bsCarousel.pause();
            video.currentTime = 0;
            video.play();
        } else {
            video.pause();
        }
    });
});
