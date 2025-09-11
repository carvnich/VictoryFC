// VictoryFC Dynamic Functionality - Using Partial Views

let map;

// Initialize Leaflet Map
function initMap() {
    const mapElement = document.getElementById('map');
    if (!mapElement) return;

    const gameLocation = mapElement.getAttribute('data-location');
    const fieldName = mapElement.getAttribute('data-field');

    if (!gameLocation) return;

    try {
        const coordinates = getLocationCoordinates(gameLocation);

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
                .bindPopup(`<b>${fieldName}</b><br>${gameLocation}`)
                .openPopup();
        } else {
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
        "355 First Rd W, Stoney Creek, ON L8E 0G5": { lat: 43.2081, lng: -79.7381 },
        "135 Fennell Ave W, Hamilton, ON L9C 1E9": { lat: 43.2501, lng: -79.9181 },
        "1145 Stone Church Rd E, Hamilton, ON L8W 3J6": { lat: 43.2067, lng: -79.7789 },
        "1500 Shaver Rd, Ancaster, ON L9G 3K9": { lat: 43.2189, lng: -79.9892 },
        "363 Wilson St E, Ancaster, ON L9G 2B8": { lat: 43.2167, lng: -79.9667 },
        "65 Herkimer St, Hamilton, ON L8P 2G5": { lat: 43.2557, lng: -79.8711 },
        "316 Sackville Hill Lane, Lower Sackville, NS B4C 2R9": { lat: 44.7761, lng: -63.6739 }
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

// Division switching functionality - now uses partial views
async function switchDivision() {
    const divisionSelect = document.getElementById('division-select');
    if (!divisionSelect) return;

    const division = divisionSelect.value;
    try {
        // Option 1: Use partial view for complete HTML consistency
        const response = await fetch(`/Home/GetStandingsPartial?division=${division}`);
        const html = await response.text();

        const standingsContainer = document.querySelector('.table-responsive');
        if (standingsContainer) {
            standingsContainer.outerHTML = html;
            initializeTooltips();
            initializeStandingsExpansion(); // Re-initialize expansion after content update
        }

        updateLastUpdateTime();
        showNotification('Standings updated successfully!', 'success');
    } catch (error) {
        console.error('Error switching division:', error);
        showNotification('Failed to load division data', 'error');
    }
}

// Competition switching functionality - now uses partial views
async function switchCompetition() {
    const competitionSelect = document.getElementById('competition-select');
    if (!competitionSelect) return;

    const competition = competitionSelect.value;
    try {
        // Use partial view for matches list
        const response = await fetch(`/Matches/GetMatchesPartial?competition=${competition}`);
        const html = await response.text();

        // Update both tabs with the same partial view logic
        const allMatchesTab = document.getElementById('all-matches');
        const victoryMatchesTab = document.getElementById('victory-matches');

        if (allMatchesTab && victoryMatchesTab) {
            // For Victory FC tab, we'd need a separate endpoint or filter the HTML
            allMatchesTab.innerHTML = html;

            // For Victory tab, make another call with filtered data
            const victoryResponse = await fetch(`/Matches/GetMatchesPartial?competition=${competition}&filter=victory`);
            const victoryHtml = await victoryResponse.text();
            victoryMatchesTab.innerHTML = victoryHtml;
        }

        showNotification('Matches updated successfully!', 'success');
    } catch (error) {
        console.error('Error switching competition:', error);
        showNotification('Failed to load competition data', 'error');
    }
}

// Alternative: Keep the old JSON approach for backwards compatibility
async function switchCompetitionJSON() {
    const competitionSelect = document.getElementById('competition-select');
    if (!competitionSelect) return;

    const competition = competitionSelect.value;
    try {
        const response = await fetch(`/Matches/GetMatches?competition=${competition}`);
        const data = await response.json();
        updateMatchesDisplay(data.matches);
        showNotification('Matches updated successfully!', 'success');
    } catch (error) {
        console.error('Error switching competition:', error);
        showNotification('Failed to load competition data', 'error');
    }
}

// Keep the old updateMatchesDisplay for fallback
function updateMatchesDisplay(matches) {
    // Same implementation as before - fallback for when partial views aren't used
    const generateMatchesHTML = (filteredMatches) => {
        const groupedMatches = filteredMatches.reduce((groups, match) => {
            const date = new Date(match.gameDate).toDateString();
            if (!groups[date]) groups[date] = [];
            groups[date].push(match);
            return groups;
        }, {});

        return Object.keys(groupedMatches)
            .sort((a, b) => new Date(b) - new Date(a))
            .map(date => {
                const dateFormatted = new Date(date).toLocaleDateString('en-US', {
                    weekday: 'long',
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric'
                });

                const matchesHTML = groupedMatches[date]
                    .sort((a, b) => new Date(a.gameDate) - new Date(b.gameDate))
                    .map(match => {
                        const gameTime = new Date(match.gameDate).toLocaleTimeString('en-US', {
                            hour: 'numeric',
                            minute: '2-digit',
                            hour12: true
                        });

                        return `
                            <div class="row align-items-center p-1 p-md-2 m-1 m-md-2 rounded match-row ${match.competition === 'spence' ? 'spence-cup' : 'regular-season'}">
                                <div class="col-4 col-md-4 text-end">
                                    <span class="fw-bold ${match.homeTeam === 'Victory FC' ? 'text-danger' : ''}">${match.homeTeam}</span>
                                </div>
                                <div class="col-4 col-md-4 text-center">
                                    ${match.isCompleted ? `
                                        <div class="d-flex align-items-center justify-content-center">
                                            <span class="fs-4 fw-bold me-2">${match.homeScore}</span>
                                            <span class="text-muted">-</span>
                                            <span class="fs-4 fw-bold ms-2">${match.awayScore}</span>
                                        </div>
                                        <div class="small text-muted">
                                            <div>${gameTime}</div>
                                            <div class="d-none d-md-block">${match.field}</div>
                                        </div>
                                    ` : `
                                        <div class="mb-0 fw-bold">vs</div>
                                        <div class="small text-muted">
                                            <div>${gameTime}</div>
                                            <div class="d-none d-md-block">${match.field}</div>
                                        </div>
                                    `}
                                </div>
                                <div class="col-4 col-md-4">
                                    <span class="fw-bold ${match.awayTeam === 'Victory FC' ? 'text-danger' : ''}">${match.awayTeam}</span>
                                </div>
                            </div>
                        `;
                    }).join('');

                return `
                    <div class="mb-4">
                        <h5 class="text-black mb-3 border-bottom pb-2">${dateFormatted}</h5>
                        ${matchesHTML}
                    </div>
                `;
            }).join('');
    };

    const allMatchesTab = document.getElementById('all-matches');
    const victoryMatchesTab = document.getElementById('victory-matches');

    if (allMatchesTab) {
        allMatchesTab.innerHTML = generateMatchesHTML(matches);
    }

    if (victoryMatchesTab) {
        const victoryMatches = matches.filter(m => m.homeTeam === 'Victory FC' || m.awayTeam === 'Victory FC');
        victoryMatchesTab.innerHTML = generateMatchesHTML(victoryMatches);
    }
}

// Refresh standings functionality
async function refreshStandings() {
    const divisionSelect = document.getElementById('division-select');
    const division = divisionSelect ? divisionSelect.value : 'regular';

    try {
        // Use partial view approach
        const response = await fetch(`/Home/GetStandingsPartial?division=${division}`);
        const html = await response.text();

        const standingsContainer = document.querySelector('.table-responsive');
        if (standingsContainer) {
            standingsContainer.outerHTML = html;
            initializeTooltips();
            initializeStandingsExpansion(); // Re-initialize expansion after refresh
        }

        updateLastUpdateTime();
        showNotification('Standings updated successfully!', 'success');
    } catch (error) {
        console.error('Error refreshing standings:', error);
        showNotification('Failed to refresh standings', 'error');
    }
}

// Initialize standings row expansion for mobile
function initializeStandingsExpansion() {
    const standingsRows = document.querySelectorAll('.standings-row');

    standingsRows.forEach(row => {
        row.addEventListener('click', function () {
            // Only work on small screens
            if (window.innerWidth >= 768) return;

            const expandedRow = row.nextElementSibling;
            const expandIcon = row.querySelector('.expand-icon');

            if (expandedRow && expandedRow.classList.contains('expanded-row')) {
                const isExpanded = expandedRow.style.display !== 'none';

                if (isExpanded) {
                    // Collapse
                    expandedRow.style.display = 'none';
                    expandIcon.classList.remove('bi-chevron-up');
                    expandIcon.classList.add('bi-chevron-down');
                    row.classList.remove('table-active');
                } else {
                    // Expand
                    expandedRow.style.display = 'table-row';
                    expandIcon.classList.remove('bi-chevron-down');
                    expandIcon.classList.add('bi-chevron-up');
                }
            }
        });
    });
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
        const existingTooltip = bootstrap.Tooltip.getInstance(tooltip);
        if (existingTooltip) {
            existingTooltip.dispose();
        }
        new bootstrap.Tooltip(tooltip);
    });
}

// Matches filtering functionality
function initializeMatchesFiltering() {
    const pillButtons = document.querySelectorAll('.nav-pills .nav-link');

    pillButtons.forEach(button => {
        button.addEventListener('click', function () {
            pillButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
        });
    });
}

// Event listeners and initialization
document.addEventListener('DOMContentLoaded', function () {
    const mapElement = document.getElementById('map');
    if (mapElement) {
        setTimeout(() => {
            if (typeof L !== 'undefined') {
                initMap();
            } else {
                const checkLeaflet = setInterval(() => {
                    if (typeof L !== 'undefined') {
                        clearInterval(checkLeaflet);
                        initMap();
                    }
                }, 100);
            }
        }, 500);
    }

    updateCountdown();
    setInterval(updateCountdown, 1000);

    initializeTooltips();
    initializeMatchesFiltering();
    initializeStandingsExpansion(); // Initialize on page load

    const divisionSelect = document.getElementById('division-select');
    if (divisionSelect) {
        divisionSelect.addEventListener('change', switchDivision);
    }

    const competitionSelect = document.getElementById('competition-select');
    if (competitionSelect) {
        // Use partial view approach by default, fallback to JSON if needed
        competitionSelect.addEventListener('change', switchCompetition);
    }

    const refreshBtn = document.getElementById('refresh-standings-btn');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', refreshStandings);
    }

    setInterval(refreshStandings, 5 * 60 * 1000);
});

// Make functions globally available
window.initMap = initMap;
window.switchDivision = switchDivision;
window.refreshStandings = refreshStandings;
window.switchCompetition = switchCompetition;