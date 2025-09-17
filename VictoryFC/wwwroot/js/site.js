// Global variables
let map;

function navigateToSection(sectionId, event) {
    // Check if we're on the home page
    if (window.location.pathname === '/') {
        // On home page - prevent default and scroll to section
        event.preventDefault();
        scrollToSection(sectionId);
    }
    // If not on home page, let the default href behavior handle navigation to home page with hash
    // The browser will automatically scroll to the section after navigation
}

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

    // Re-initialize collapse functionality
    initializeScorersCollapse();
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

async function switchCompetitionMatches() {
    const select = document.getElementById('competition-select');
    if (!select) return;

    // Update all matches tab
    const allResponse = await fetch(`/Match/GetMatchesPartial?competition=${select.value}`);
    const allHtml = await allResponse.text();
    const allTab = document.getElementById('all-matches');
    if (allTab) allTab.innerHTML = allHtml;

    // Update Victory FC matches tab
    const victoryResponse = await fetch(`/Match/GetMatchesPartial?competition=${select.value}&filter=victory`);
    const victoryHtml = await victoryResponse.text();
    const victoryTab = document.getElementById('victory-matches');
    if (victoryTab) victoryTab.innerHTML = victoryHtml;

    // Re-apply edit mode visibility
    const editToggle = document.getElementById('editModeToggle');
    if (editToggle?.checked) {
        const editButtons = document.querySelectorAll('.match-edit-btn');
        editButtons.forEach(btn => btn.style.display = 'block');
    }
}

function populateMatchesCarousel(allMatches) {
    const carouselInner = document.getElementById('matches-carousel-inner');
    const carouselIndicators = document.getElementById('matches-carousel-indicators');
    const navButtons = document.querySelectorAll('.carousel-nav-btn');

    if (!carouselInner) return;

    const today = new Date();

    // Group matches by date
    const matchesByDate = allMatches.reduce((acc, match) => {
        const matchDate = new Date(match.date);
        const dateKey = matchDate.toDateString();
        if (!acc[dateKey]) acc[dateKey] = [];
        acc[dateKey].push(match);
        return acc;
    }, {});

    // Get all match dates (both upcoming and past) and sort them
    const allDates = Object.keys(matchesByDate)
        .map(date => new Date(date))
        .sort((a, b) => a - b);

    if (allDates.length === 0) {
        carouselInner.innerHTML = '<div class="carousel-item active"><div class="text-center p-4"><h5>No matches available</h5></div></div>';
        return;
    }

    // Find the next upcoming match date or today's matches
    let activeIndex = 0;
    const now = new Date();
    for (let i = 0; i < allDates.length; i++) {
        const dateMatches = matchesByDate[allDates[i].toDateString()];
        // Check if this date has upcoming matches or is today
        if (allDates[i] >= today || dateMatches.some(m => !m.isCompleted)) {
            activeIndex = i;
            break;
        }
        // If no upcoming matches found, show the last date
        if (i === allDates.length - 1) {
            activeIndex = i;
        }
    }

    // Create indicators
    if (carouselIndicators) {
        carouselIndicators.innerHTML = '';
        allDates.forEach((date, index) => {
            const indicator = document.createElement('button');
            indicator.type = 'button';
            indicator.setAttribute('data-bs-target', '#matchesCarousel');
            indicator.setAttribute('data-bs-slide-to', index.toString());
            indicator.className = index === activeIndex ? 'active' : '';
            indicator.setAttribute('aria-label', `Slide ${index + 1}`);
            if (index === activeIndex) {
                indicator.setAttribute('aria-current', 'true');
            }
            carouselIndicators.appendChild(indicator);
        });
    }

    // Update navigation button states
    function updateNavButtons(currentIndex) {
        navButtons.forEach((btn, i) => {
            if (i === 0) { // Previous button
                btn.disabled = currentIndex === 0;
            } else { // Next button
                btn.disabled = currentIndex === allDates.length - 1;
            }
        });
    }

    allDates.forEach((date, index) => {
        const matches = matchesByDate[date.toDateString()].sort((a, b) => new Date(a.date) - new Date(b.date));
        const isActive = index === activeIndex ? 'active' : '';

        const carouselItem = document.createElement('div');
        carouselItem.className = `carousel-item ${isActive}`;

        const matchesHtml = matches.map(match => `
            <div class="row align-items-center p-2 m-2 rounded match-row ${match.competitionClass}">
                <div class="col-4 text-end">
                    <span class="${match.homeTeam === 'Victory FC' ? 'text-vfc-red' : ''}">${match.homeTeam}</span>
                </div>
                <div class="col-4 text-center">
                    ${match.isCompleted ?
                `<div class="d-flex align-items-center justify-content-center">
                            <span class="fs-4 me-2">${match.homeScore}</span>
                            <span class="text-muted">-</span>
                            <span class="fs-4 ms-2">${match.awayScore}</span>
                        </div>` :
                '<div class="mb-0">vs</div>'
            }
                    <div class="small text-muted">
                        <div>${match.formattedTime}</div>
                        <div class="d-none d-md-block">${match.field}</div>
                        <div class="d-md-none small">${match.field}</div>
                    </div>
                </div>
                <div class="col-4">
                    <span class="${match.awayTeam === 'Victory FC' ? 'text-vfc-red' : ''}">${match.awayTeam}</span>
                </div>
            </div>
        `).join('');

        carouselItem.innerHTML = `
            <div class="text-center mb-3">
                <h5 class="mb-0">${date.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}</h5>
            </div>
            ${matchesHtml}
        `;

        carouselInner.appendChild(carouselItem);
    });

    // Initial button state
    updateNavButtons(activeIndex);

    // Listen for carousel slide events to update button states
    const carousel = document.getElementById('matchesCarousel');
    if (carousel) {
        carousel.addEventListener('slid.bs.carousel', (event) => {
            updateNavButtons(event.to);
        });
    }
}

function editMatch(matchId) {
    // Placeholder function for future CRUD operations
    alert(`Edit match ${matchId} - CRUD functionality coming soon!`);
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

function initializeScorersCollapse() {
    const scorersIcon = document.querySelector('.scorers-expand-icon');
    const scorersText = document.querySelector('.scorers-expand-text');
    const scorersCollapse = document.getElementById('scorers-collapse');

    if (scorersIcon && scorersCollapse) {
        scorersCollapse.addEventListener('show.bs.collapse', () => {
            scorersIcon.className = 'bi bi-chevron-up text-muted scorers-expand-icon';
            if (scorersText) scorersText.textContent = 'Show Less';
        });

        scorersCollapse.addEventListener('hide.bs.collapse', () => {
            scorersIcon.className = 'bi bi-chevron-down text-muted scorers-expand-icon';
            if (scorersText) scorersText.textContent = 'Show More';
        });
    }
}

function initializeEditMode() {
    const editToggle = document.getElementById('editModeToggle');
    if (editToggle) {
        editToggle.addEventListener('change', function () {
            const editButtons = document.querySelectorAll('.match-edit-btn');
            editButtons.forEach(btn => {
                btn.style.display = this.checked ? 'block' : 'none';
            });
        });
    }
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
    initializeScorersCollapse();
    initializeEditMode();
    createScrollToTopButton();

    // Attach dropdown event listeners for home page
    document.getElementById('division-select')?.addEventListener('change', switchDivision);
    document.getElementById('scorers-select')?.addEventListener('change', switchScorers);

    // Check if we're on home page or matches page for competition dropdown
    const currentPath = window.location.pathname.toLowerCase();
    if (currentPath === '/' || currentPath.includes('/home')) {
        document.getElementById('competition-select')?.addEventListener('change', switchCompetition);
    } else if (currentPath.includes('/match')) {
        document.getElementById('competition-select')?.addEventListener('change', switchCompetitionMatches);
    }

    // Initialize carousel on home page
    if (window.matchesData && document.getElementById('matches-carousel-inner')) {
        populateMatchesCarousel(window.matchesData);
    }
});