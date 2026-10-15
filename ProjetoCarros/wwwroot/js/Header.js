const btn = document.getElementById('mobileMenuToggle');
const overlay = document.getElementById('mobileMenuOverlay');

if (btn && overlay) {
    btn.addEventListener('click', function () {
        overlay.classList.toggle('open');
    });
}