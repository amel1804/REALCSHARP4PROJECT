/**
 * JavaScript global de l'application Basketball LiveScore
 * Fonctions utilitaires communes à toutes les pages
 */

// Configuration de base de l'API
const API_CONFIG = {
    baseUrl: '/api',
    timeout: 30000
};

/**
 * Fonction helper pour les appels API avec gestion du token JWT
 * Basée sur les exemples du cours HttpClient
 */
async function callApi(endpoint, method = 'GET', data = null) {
    // Récupération du token depuis la session (géré côté serveur)
    const url = `${API_CONFIG.baseUrl}/${endpoint}`;

    const options = {
        method: method,
        headers: {
            'Content-Type': 'application/json'
        }
    };

    // Ajout du body pour POST/PUT
    if (data && (method === 'POST' || method === 'PUT')) {
        options.body = JSON.stringify(data);
    }

    try {
        const response = await fetch(url, options);

        if (!response.ok) {
            throw new Error(`Erreur HTTP: ${response.status}`);
        }

        // Retourner le JSON si la réponse en contient
        const contentType = response.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            return await response.json();
        }

        return response;
    } catch (error) {
        console.error('Erreur API:', error);
        throw error;
    }
}

/**
 * Afficher une notification temporaire
 * Utilise Bootstrap pour le style
 */
function showNotification(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed top-0 end-0 m-3`;
    alertDiv.style.zIndex = '9999';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(alertDiv);

    // Suppression automatique après 5 secondes
    setTimeout(() => {
        alertDiv.remove();
    }, 5000);
}

/**
 * Formater une date pour l'affichage français
 */
function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('fr-FR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

/**
 * Initialisation au chargement du DOM
 */
document.addEventListener('DOMContentLoaded', function () {
    // Activation des tooltips Bootstrap si présents
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    if (tooltipTriggerList.length > 0) {
        [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    }
});