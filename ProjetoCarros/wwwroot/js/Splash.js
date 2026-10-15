document.addEventListener('DOMContentLoaded', function () {
    const logo = document.getElementById('splash-logo').querySelector('img');
    const overlay = document.getElementById('splash-overlay');

    setTimeout(() => {
        logo.style.transform = 'scale(1)';
        logo.style.opacity = '1';
    }, 100);

    setTimeout(() => {
        overlay.classList.add('hide');
    }, 2000);

    setTimeout(() => {
        overlay.remove();
    }, 2800);
});