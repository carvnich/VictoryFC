// VictoryFC Dynamic Functionality

let map;

// Initialize Leaflet Map
function initMap() {
    const mapElement = document.getElementById('map');
    if (!mapElement) return;

    const gameLocation = mapElement.getAttribute('data-location');
    const fieldName = mapElement.getAttribute('data-field');

    if (!gameLocation) return;

    try {
        // Get coordinates for the location
        const coordinates = getLocationCoordinates(gameLocation);

        if (coordinates) {
            // Initialize Leaflet map
            map = L.map('map').setView([coordinates.lat, coordinates.lng], 15);

            // Add OpenStreetMap tiles
            L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19,
                attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            }).addTo(map);

            // Add custom marker
            const customIcon = L.divIcon({
                className: 'custom-map-marker',
                html: '<i class="bi bi-geo-alt-fill text-danger" style="font-size: 2rem;"></i>',
                iconSize: [30, 30],
                iconAnchor: [15, 30]
            });

            // Add marker to map
            L.marker([coordinates.lat, coordinates.lng], { icon: customIcon })
                .addTo(map)
                .bindPopup(`<b>${fieldName}</b><br>${gameLocation}`)
                .openPopup();
        } else {
            // Fallback to placeholder
            showMapFallback(mapElement, fieldName);
        }
    } catch (error) {
        console.error('Error initializing map:', error);
        showMapFallback(mapElement, fieldName);
    }
}

// Get coordinates for known locations
function getLocationCoordinates(location) {
    const locationMap = {
        "355 First Rd W, Stoney Creek, ON L8E 0G5": { lat: 43.2081, lng: -79.7381 }, // Heritage Green
        "135 Fennell Ave W, Hamilton, ON L9C 1E9": { lat: 43.2501, lng: -79.9181 }, // Mohawk Sports Park
        "1145 Stone Church Rd E, Hamilton, ON L8W 3J6": { lat: 43.2067, lng: -79.7789 }, // Hamilton Italian Centre
        "1500 Shaver Rd, Ancaster, ON L9G 3K9": { lat: 43.2189, lng: -79.9892 }, // Shady Acres
        "363 Wilson St E, Ancaster, ON L9G 2B8": { lat: 43.2167, lng: -79.9667 }, // Ancaster
        "65 Herkimer St, Hamilton, ON L8P 2G5": { lat: 43.2557, lng: -79.8711 }, // Proto Field
        "316 Sackville Hill Lane, Lower Sackville, NS B4C 2R9": { lat: 44.7761, lng: -63.6739 } // Sackville
    };

    return locationMap[location] || null;
}

// Show fallback placeholder for map
function showMapFallback(mapElement, fieldName) {
    mapElement.innerHTML = `
        <div class="bg-light rounded d-flex align-items-center justify-content-center h-100">
            <div class="text-muted text-center">
                <i class="bi bi-map display-3"></i>
                <p class="mt-3 mb-0">Stadium Location Map</p>
                <small class="text-muted">${fieldName}</small>
            </div>
        </div>`;
}

// Game countdown functionality
function updateCountdown() {
    const countdownElement = document.getElementById('countdown');
    if (!countdownElement) return;

    const target = countdownElement.getAttribute('data-target');
    if (!target) return;

    const gameDate = new Date(target).getTime();
    const now = new Date().getTime();
    const distance = gameDate - now;

    if (distance > 0) {
        const days = Math.floor(distance / (1000 * 60 * 60 * 24));
        const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((distance % (1000 * 60)) / 1000);

        const daysEl = document.getElementById('days');
        const hoursEl = document.getElementById('hours');
        const minutesEl = document.getElementById('minutes');
        const secondsEl = document.getElementById('seconds');

        if (daysEl) daysEl.textContent = days.toString().padStart(2, '0');
        if (hoursEl) hoursEl.textContent = hours.toString().padStart(2, '0');
        if (minutesEl) minutesEl.textContent = minutes.toString().padStart(2, '0');
        if (secondsEl) secondsEl.textContent = seconds.toString().padStart(2, '0');
    } else {
        countdownElement.innerHTML = '<div class="display-6 fw-bold">GAME DAY!</div>';
    }
}

// Division switching functionality
async function switchDivision() {
    const divisionSelect = document.getElementById('division-select');
    if (!divisionSelect) return;

    const division = divisionSelect.value;
    try {
        const response = await fetch(`/Home/GetStandings?division=${division}`);
        const data = await response.json();
        updateStandingsTable(data.standings);
        updateLastUpdateTime();
    } catch (error) {
        console.error('Error switching division:', error);
        showNotification('Failed to load division data', 'error');
    }
}

// Refresh standings functionality
async function refreshStandings() {
    const divisionSelect = document.getElementById('division-select');
    const division = divisionSelect ? divisionSelect.value : 'regular';

    try {
        const response = await fetch(`/Home/GetStandings?division=${division}`);
        const data = await response.json();
        updateStandingsTable(data.standings);
        updateLastUpdateTime();
        showNotification('Standings updated successfully!', 'success');
    } catch (error) {
        console.error('Error refreshing standings:', error);
        showNotification('Failed to refresh standings', 'error');
    }
}

// Update standings table
function updateStandingsTable(standings) {
    const tbody = document.querySelector('#standings-table tbody');
    if (!tbody) return;

    tbody.innerHTML = '';

    standings.forEach((team, index) => {
        //const rowClass = team.team === 'VICTORY FC' ? 'table-primary' : 'bg-light';
        const row = document.createElement('tr');
        //row.className = `${rowClass} rounded`;
        row.setAttribute('data-team', team.team);

        row.innerHTML = `
            <td class="text-dark border-0">${index + 1} &nbsp;&nbsp; ${team.team}</td>
            <td class="text-center text-dark border-0">${team.p}</td>
            <td class="text-center text-dark border-0">${team.w}</td>
            <td class="text-center text-dark border-0">${team.d}</td>
            <td class="text-center text-dark border-0">${team.l}</td>
            <td class="text-center text-dark border-0 d-none d-md-table-cell">${team.gf}</td>
            <td class="text-center text-dark border-0 d-none d-md-table-cell">${team.ga}</td>
            <td class="text-center text-dark border-0">${team.gd >= 0 ? '+' : ''}${team.gd}</td>
            <td class="text-center text-dark border-0">${team.pts}</td>
            <td class="text-center border-0">${generateLastFiveHTML(team.lastFiveMatches)}</td>
        `;

        tbody.appendChild(row);
    });

    initializeTooltips();
}

// Generate last five matches HTML
function generateLastFiveHTML(matches) {
    if (!matches || !Array.isArray(matches)) return '';

    return matches.map(match => {
        const bgColor = match.isWin ? '#24a700' : match.isDraw ? '#6c757d' : '#ff0000';
        const icon = match.isWin ? 'bi-check' : match.isDraw ? 'bi-dash' : 'bi-x';
        return `<span class="d-inline-flex align-items-center justify-content-center rounded-circle me-1 match-result" style="width:20px;height:20px;background:${bgColor}" data-bs-toggle="tooltip" title="${match.opponent}"><i class="bi ${icon} text-white" style="display: flex; font-size:14px"></i></span>`;
    }).join('');
}

// Show notification
function showNotification(message, type) {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'success' ? 'success' : 'danger'} position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 1050; min-width: 300px;';
    notification.innerHTML = `
        <i class="bi ${type === 'success' ? 'bi-check-circle' : 'bi-exclamation-triangle'} me-2"></i>
        ${message}
        <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
    `;

    document.body.appendChild(notification);
    setTimeout(() => notification.remove(), 3000);
}

// Update last update time
function updateLastUpdateTime() {
    const updateElement = document.getElementById('standings-last-update');
    if (!updateElement) return;

    const now = new Date();
    const timeString = now.toLocaleDateString('en-US', {
        month: 'short',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
    });
    updateElement.textContent = timeString;
}

// Initialize tooltips
function initializeTooltips() {
    const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltips.forEach(tooltip => {
        // Dispose existing tooltip if it exists
        const existingTooltip = bootstrap.Tooltip.getInstance(tooltip);
        if (existingTooltip) {
            existingTooltip.dispose();
        }
        // Create new tooltip
        new bootstrap.Tooltip(tooltip);
    });
}

// Event listeners and initialization
document.addEventListener('DOMContentLoaded', function () {
    // Initialize map if element exists
    const mapElement = document.getElementById('map');
    if (mapElement) {
        // Wait a bit for Leaflet to fully load
        setTimeout(() => {
            if (typeof L !== 'undefined') {
                initMap();
            } else {
                // Wait for Leaflet to load
                const checkLeaflet = setInterval(() => {
                    if (typeof L !== 'undefined') {
                        clearInterval(checkLeaflet);
                        initMap();
                    }
                }, 100);
            }
        }, 500);
    }

    // Initialize countdown
    updateCountdown();
    setInterval(updateCountdown, 1000);

    // Initialize tooltips
    initializeTooltips();

    // Set up event listeners
    const divisionSelect = document.getElementById('division-select');
    if (divisionSelect) {
        divisionSelect.addEventListener('change', switchDivision);
    }

    const refreshBtn = document.getElementById('refresh-standings-btn');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', refreshStandings);
    }

    // Auto-refresh standings every 5 minutes
    setInterval(refreshStandings, 5 * 60 * 1000);
});

// Make functions globally available
window.initMap = initMap;
window.switchDivision = switchDivision;
window.refreshStandings = refreshStandings;