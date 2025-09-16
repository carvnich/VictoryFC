// Global variables
let map;

function scrollToSection(sectionId) {
    const element = document.getElementById(sectionId);
    if (element) {
        const navbar = document.querySelector('.navbar');
        const offset = navbar ? navbar.offsetHeight + 20 : 80;

        window.scrollTo({
            top: element.offsetTop - offset,
            behavior: 'smooth'
        });

        // Close mobile navbar
        const navbarCollapse = document.getElementById('navbarNav');
        if (navbarCollapse?.classList.contains('show')) {
            bootstrap.Collapse.getInstance(navbarCollapse)?.hide();
        }
    }
}

function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function initMap() {
    const mapElement = document.getElementById('map');
    if (!mapElement) return;

    const location = mapElement.getAttribute('data-location');
    const fieldName = mapElement.getAttribute('data-field');
    const coords = { lat: 43.2081, lng: -79.7381 }; // Heritage Green default

    map = L.map('map').setView([coords.lat, coords.lng], 15);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    // Custom Victory FC red marker
    const redIcon = L.divIcon({
        className: 'custom-map-marker',
        html: '<i class="bi bi-geo-alt-fill" style="color: #df2c30; font-size: 2rem; filter: drop-shadow(2px 2px 4px rgba(0,0,0,0.3));"></i>',
        iconSize: [30, 30],
        iconAnchor: [15, 30]
    });

    L.marker([coords.lat, coords.lng], { icon: redIcon })
        .addTo(map)
        .bindPopup(`<b>${fieldName}</b><br>${location}`)
        .openPopup();
}

function updateCountdown() {
    const countdown = document.getElementById('countdown');
    if (!countdown) return;

    const target = new Date(countdown.getAttribute('data-target')).getTime();
    const now = new Date().getTime();
    const distance = target - now;

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
        countdown.innerHTML = '<div class="display-6 fw-bold">MATCH DAY!</div>';
    }
}

async function handleDropdownChange(selectId, targetContainer, endpoint) {
    const select = document.getElementById(selectId);
    if (!select) return;

    const response = await fetch(`/Home/GetPartial?type=${endpoint}&value=${select.value}`);
    const html = await response.text();

    const container = document.querySelector(targetContainer);
    if (container) container.outerHTML = html;
}

async function switchDivision() {
    const select = document.getElementById('division-select');
    if (!select) return;

    const response = await fetch(`/Home/GetStandingsPartial?division=${select.value}`);
    const html = await response.text();
    document.querySelector('.table-responsive').outerHTML = html;

    // Re-initialize after AJAX update
    initializeTooltips();
    initializeStandingsExpansion();
}

async function switchScorers() {
    const select = document.getElementById('scorers-select');
    if (!select) return;

    const response = await fetch(`/Home/GetScorersPartial?competition=${select.value}`);
    const html = await response.text();
    document.getElementById('scorers-container').innerHTML = html;
}

async function switchCompetition() {
    const select = document.getElementById('competition-select');
    if (!select) return;

    // Update all matches tab
    const allResponse = await fetch(`/Home/GetMatchesPartial?competition=${select.value}`);
    const allHtml = await allResponse.text();
    const allTab = document.getElementById('all-matches');
    if (allTab) allTab.innerHTML = allHtml;

    // Update Victory FC matches tab
    const victoryResponse = await fetch(`/Home/GetMatchesPartial?competition=${select.value}&filter=victory`);
    const victoryHtml = await victoryResponse.text();
    const victoryTab = document.getElementById('victory-matches');
    if (victoryTab) victoryTab.innerHTML = victoryHtml;
}

function initializeTooltips() {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(tooltip => {
        const existing = bootstrap.Tooltip.getInstance(tooltip);
        if (existing) existing.dispose(); // Clean up existing tooltips
        new bootstrap.Tooltip(tooltip);
    });
}

function initializeStandingsExpansion() {
    document.querySelectorAll('.standings-row').forEach(row => {
        row.addEventListener('click', function () {
            if (window.innerWidth >= 768) return; // Only on mobile

            const expandedRow = row.nextElementSibling;
            const expandIcon = row.querySelector('.expand-icon');

            if (expandedRow?.classList.contains('expanded-row')) {
                const isExpanded = expandedRow.style.display !== 'none';

                expandedRow.style.display = isExpanded ? 'none' : 'table-row';
                if (expandIcon) {
                    expandIcon.className = `bi ${isExpanded ? 'bi-chevron-down' : 'bi-chevron-up'} d-md-none text-muted ms-0 expand-icon`;
                }
            }
        });
    });
}

function createScrollToTopButton() {
    const scrollButton = document.createElement('button');
    scrollButton.className = 'btn vfc-loss rounded-circle position-fixed';
    scrollButton.style.cssText = 'bottom: 20px; right: 20px; width: 50px; height: 50px; z-index: 1000; display: none;';
    scrollButton.innerHTML = '<i class="bi bi-arrow-up"></i>';
    scrollButton.onclick = scrollToTop;
    document.body.appendChild(scrollButton);

    // Show/hide scroll button based on scroll position
    window.addEventListener('scroll', () => {
        scrollButton.style.display = window.pageYOffset > 300 ? 'block' : 'none';
    });
}

document.addEventListener('DOMContentLoaded', function () {
    // Initialize countdown timer
    updateCountdown();
    setInterval(updateCountdown, 1000);

    // Initialize map if available
    if (document.getElementById('map') && typeof L !== 'undefined') {
        setTimeout(initMap, 500);
    }

    // Initialize UI components
    initializeTooltips();
    initializeStandingsExpansion();
    createScrollToTopButton();

    // Attach dropdown event listeners
    document.getElementById('division-select')?.addEventListener('change', switchDivision);
    document.getElementById('scorers-select')?.addEventListener('change', switchScorers);
    document.getElementById('competition-select')?.addEventListener('change', switchCompetition);
});