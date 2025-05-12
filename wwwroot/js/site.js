// Funzione per animare i contatori
function animateCounters() {
    const counters = document.querySelectorAll('.counter');

    counters.forEach(counter => {
        const target = parseInt(counter.innerText);
        let count = 0;
        const increment = Math.ceil(target / 20);

        function updateCount() {
            if (count < target) {
                count += increment;
                if (count > target) count = target;
                counter.innerText = count;
                setTimeout(updateCount, 50);
            }
        }

        updateCount();
    });
}

// Funzione per le animazioni di fade-in
function initFadeAnimations() {
    const fadeElements = document.querySelectorAll('.fade-in');

    fadeElements.forEach((el, index) => {
        // Se non ha già un delay personalizzato, aggiungiamone uno basato sull'indice
        const delay = el.style.getPropertyValue('--delay') || `${index * 0.1}s`;
        el.style.animation = `fadeIn 0.8s ease ${delay} forwards`;
    });
}

// Inizializza i tooltip di Bootstrap
function initTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Aggiunge l'evidenziazione alla voce del menu corrente
function highlightCurrentNavItem() {
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');

    navLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href && currentPath.startsWith(href) && href !== '/') {
            link.classList.add('active');
            link.setAttribute('aria-current', 'page');
        } else if (href === '/' && currentPath === '/') {
            link.classList.add('active');
            link.setAttribute('aria-current', 'page');
        }
    });
}

// Funzioni per l'API Gemini
function initGeminiAI() {
    // Miglioramento descrizione competenza
    const improveBtn = document.getElementById('improveBtn');
    if (improveBtn) {
        improveBtn.addEventListener('click', function () {
            const skillName = document.getElementById('skillName').value;
            const description = document.getElementById('skillDescription').value;
            const resultContainer = document.getElementById('improveResultContainer');
            const resultDiv = document.getElementById('improveResult');
            const errorDiv = document.getElementById('improveError');

            if (!skillName) {
                errorDiv.textContent = "Inserisci il nome della competenza";
                resultContainer.style.display = "block";
                resultDiv.textContent = "";
                return;
            }

            resultContainer.style.display = "block";
            resultDiv.innerHTML = "<div class='spinner-border spinner-border-sm' role='status'></div> Generazione in corso...";
            errorDiv.textContent = "";

            // IMPORTANTE: Formato URL corretto per Razor Pages
            fetch(`/api/gemini?handler=improvedescription&skillName=${encodeURIComponent(skillName)}&description=${encodeURIComponent(description)}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`Errore HTTP: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    resultDiv.textContent = data.description || "Nessuna descrizione generata.";
                })
                .catch(error => {
                    console.error('Errore:', error);
                    errorDiv.textContent = `Errore: ${error.message}`;
                    resultDiv.textContent = "";
                });
        });
    }

    // Generazione spunti conversazione
    const conversationBtn = document.getElementById('conversationBtn');
    if (conversationBtn) {
        conversationBtn.addEventListener('click', function () {
            const skill1 = document.getElementById('skill1').value;
            const skill2 = document.getElementById('skill2').value;
            const resultContainer = document.getElementById('conversationResultContainer');
            const resultDiv = document.getElementById('conversationResult');
            const errorDiv = document.getElementById('conversationError');

            if (!skill1 || !skill2) {
                errorDiv.textContent = "Inserisci entrambe le competenze";
                resultContainer.style.display = "block";
                resultDiv.textContent = "";
                return;
            }

            resultContainer.style.display = "block";
            resultDiv.innerHTML = "<div class='spinner-border spinner-border-sm' role='status'></div> Generazione in corso...";
            errorDiv.textContent = "";

            // IMPORTANTE: Formato URL corretto per Razor Pages
            fetch(`/api/gemini?handler=conversationstarter&skill1=${encodeURIComponent(skill1)}&skill2=${encodeURIComponent(skill2)}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`Errore HTTP: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    resultDiv.textContent = data.starter || "Nessuno spunto generato.";
                })
                .catch(error => {
                    console.error('Errore:', error);
                    errorDiv.textContent = `Errore: ${error.message}`;
                    resultDiv.textContent = "";
                });
        });
    }

    // Suggerimenti personalizzati
    const suggestionBtn = document.getElementById('suggestionBtn');
    if (suggestionBtn) {
        suggestionBtn.addEventListener('click', function () {
            const skills = document.getElementById('skillsList').value;
            const resultContainer = document.getElementById('suggestionResultContainer');
            const resultDiv = document.getElementById('suggestionResult');
            const errorDiv = document.getElementById('suggestionError');

            if (!skills) {
                errorDiv.textContent = "Inserisci almeno una competenza";
                resultContainer.style.display = "block";
                resultDiv.textContent = "";
                return;
            }

            resultContainer.style.display = "block";
            resultDiv.innerHTML = "<div class='spinner-border spinner-border-sm' role='status'></div> Generazione in corso...";
            errorDiv.textContent = "";

            // IMPORTANTE: Formato URL corretto per Razor Pages
            fetch(`/api/gemini?handler=suggestion&skills=${encodeURIComponent(skills)}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`Errore HTTP: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    resultDiv.textContent = data.suggestion || "Nessun suggerimento generato.";
                })
                .catch(error => {
                    console.error('Errore:', error);
                    errorDiv.textContent = `Errore: ${error.message}`;
                    resultDiv.textContent = "";
                });
        });
    }
}

// Inizializza tutte le funzioni quando il DOM è caricato
document.addEventListener('DOMContentLoaded', function () {
    animateCounters();
    initFadeAnimations();
    initTooltips();
    highlightCurrentNavItem();
    initGeminiAI(); // Aggiunta questa chiamata per inizializzare le funzioni Gemini
});

// Script per refresh citazione
document.getElementById('refreshQuoteBtn')?.addEventListener('click', function () {
    const quoteText = document.getElementById('quoteText');
    const quoteAuthor = document.getElementById('quoteAuthor');
    const refreshBtn = document.getElementById('refreshQuoteBtn');

    // Disabilita il pulsante e mostra l'animazione di caricamento
    refreshBtn.disabled = true;
    refreshBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';

    // Recupera una nuova citazione
    fetch('/api/quote?handler=refresh') // Modificato per usare il formato corretto di Razor Pages
        .then(response => response.json())
        .then(data => {
            quoteText.textContent = data.quote;
            quoteAuthor.textContent = data.author;

            // Ripristina il pulsante
            refreshBtn.disabled = false;
            refreshBtn.innerHTML = '<i class="bi bi-arrow-clockwise"></i>';
        })
        .catch(error => {
            console.error('Errore nel recuperare la citazione:', error);
            // Ripristina il pulsante anche in caso di errore
            refreshBtn.disabled = false;
            refreshBtn.innerHTML = '<i class="bi bi-arrow-clockwise"></i>';
        });
});

// Service Worker Registration (commentato in fase di sviluppo per evitare problemi)
/*
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/service-worker.js')
            .then(reg => console.log('Service worker registrato!', reg))
            .catch(err => console.log('Errore registrazione service worker:', err));
    });
}
*/