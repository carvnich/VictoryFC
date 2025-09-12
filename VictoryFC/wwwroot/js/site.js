// VictoryFC - Simplified functionality

let map;

// Initialize Leaflet Map
function initMap() {
    const mapElement = document.getElementById('map');
    if (!mapElement) return;

    const location = mapElement.getAttribute('data-location');
    const fieldName = mapElement.getAttribute('data-field');
    const coordinates = getLocationCoordinates(location);

    if (coordinates) {
        map = L.map('map').setView([coordinates.lat, coordinates.lng], 15);
        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(map);

        const customIcon = L.divIcon({
            className: 'custom-map-marker',
            html: '<i class="bi bi-geo-alt-fill text-danger" style="font-size: 2rem;"></i>',
            iconSize: [30, 30],
            iconAnchor: [15, 30]
        });

        L.marker([coordinates.lat, coordinates.lng], { icon: customIcon })
            .addTo(map)
            .bindPopup(`<b>${fieldName}</b><br>${location}`)
            .openPopup();
    } else {
        mapElement.innerHTML = `
            <div class="bg-light rounded d-flex align-items-center justify-content-center h-100">
                <div class="text-muted text-center">
                    <i class="bi bi-map display-3"></i>
                    <p class="mt-3 mb-0">Stadium Location Map</p>
                    <small class="text-muted">${fieldName}</small>
                </div>
            </div>`;
    }
}

function getLocationCoordinates(location) {
    const locations = {
        "355 First Rd W, Stoney Creek, ON L8E 0G5": { lat: 43.2081, lng: -79.7381 },
        "135 Fennell Ave W, Hamilton, ON L9C 1E9": { lat: 43.2501, lng: -79.9181 },
        "1145 Stone Church Rd E, Hamilton, ON L8W 3J6": { lat: 43.2067, lng: -79.7789 },
        "1500 Shaver Rd, Ancaster, ON L9G 3K9": { lat: 43.2189, lng: -79.9892 },
        "363 Wilson St E, Ancaster, ON L9G 2B8": { lat: 43.2167, lng: -79.9667 },
        "65 Herkimer St, Hamilton, ON L8P 2G5": { lat: 43.2557, lng: -79.8711 },
        "316 Sackville Hill Lane, Lower Sackville, NS B4C 2R9": { lat: 44.7761, lng: -63.6739 }
    };
    return locations[location] || null;
}

// Match countdown
function updateCountdown() {
    const countdown = document.getElementById('countdown');
    if (!countdown) return;

    const target = countdown.getAttribute('data-target');
    if (!target) return;

    const distance = new Date(target).getTime() - new Date().getTime();

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

// Switch division standings
async function switchDivision() {
    const select = document.getElementById('division-select');
    if (!select) return;

    const response = await fetch(`/Home/GetStandingsPartial?division=${select.value}`);
    const html = await response.text();

    document.querySelector('.table-responsive').outerHTML = html;
    initializeTooltips();
    initializeStandingsExpansion();
}

// Switch competition matches
async function switchCompetition() {
    const select = document.getElementById('competition-select');
    if (!select) return;

    const response = await fetch(`/Matches/GetMatchesPartial?competition=${select.value}`);
    const html = await response.text();

    const allTab = document.getElementById('all-matches');
    if (allTab) allTab.innerHTML = html;

    // Get Victory FC filtered matches
    const victoryResponse = await fetch(`/Matches/GetMatchesPartial?competition=${select.value}&filter=victory`);
    const victoryHtml = await victoryResponse.text();

    const victoryTab = document.getElementById('victory-matches');
    if (victoryTab) victoryTab.innerHTML = victoryHtml;
}

// Initialize mobile standings expansion
function initializeStandingsExpansion() {
    document.querySelectorAll('.standings-row').forEach(row => {
        row.addEventListener('click', function () {
            if (window.innerWidth >= 768) return;

            const expandedRow = row.nextElementSibling;
            const expandIcon = row.querySelector('.expand-icon');

            if (expandedRow?.classList.contains('expanded-row')) {
                const isExpanded = expandedRow.style.display !== 'none';

                expandedRow.style.display = isExpanded ? 'none' : 'table-row';
                expandIcon.className = `bi ${isExpanded ? 'bi-chevron-down' : 'bi-chevron-up'} d-md-none text-muted ms-2 expand-icon`;
            }
        });
    });
}

// Initialize tooltips
function initializeTooltips() {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(tooltip => {
        const existing = bootstrap.Tooltip.getInstance(tooltip);
        if (existing) existing.dispose();
        new bootstrap.Tooltip(tooltip);
    });
}

// Initialize everything on page load
document.addEventListener('DOMContentLoaded', function () {
    // Map initialization
    if (document.getElementById('map')) {
        setTimeout(() => {
            if (typeof L !== 'undefined') initMap();
        }, 500);
    }

    // Countdown
    updateCountdown();
    setInterval(updateCountdown, 1000);

    // Initialize UI components
    initializeTooltips();
    initializeStandingsExpansion();

    // Event listeners
    document.getElementById('division-select')?.addEventListener('change', switchDivision);
    document.getElementById('competition-select')?.addEventListener('change', switchCompetition);

    // Auto refresh standings every 5 minutes
    setInterval(switchDivision, 5 * 60 * 1000);
});