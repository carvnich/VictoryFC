// Game countdown functionality
function updateCountdown() {
    // Set next game date (September 14, 2025 at 11:00 AM)
    const gameDate = new Date('2025-09-14T11:00:00').getTime();
    const now = new Date().getTime();
    const distance = gameDate - now;

    if (distance > 0) {
        const days = Math.floor(distance / (1000 * 60 * 60 * 24));
        const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((distance % (1000 * 60)) / 1000);

        document.getElementById('days').textContent = days.toString().padStart(2, '0');
        document.getElementById('hours').textContent = hours.toString().padStart(2, '0');
        document.getElementById('minutes').textContent = minutes.toString().padStart(2, '0');
        document.getElementById('seconds').textContent = seconds.toString().padStart(2, '0');
    } else {
        document.getElementById('countdown').innerHTML = '<div class="display-6 fw-bold">GAME DAY!</div>';
    }
}

// Initialize when page loads
document.addEventListener('DOMContentLoaded', function () {
    // Start countdown
    updateCountdown();
    setInterval(updateCountdown, 1000);

    // Initialize tooltips
    const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltips.forEach(tooltip => {
        new bootstrap.Tooltip(tooltip);
    });
});