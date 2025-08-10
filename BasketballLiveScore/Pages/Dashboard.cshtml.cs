using BasketballLiveScore.DTOs.Match;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace BasketballLiveScore.Pages
{
    /// <summary>
    /// Page du tableau de bord principal
    /// Affiche un résumé complet de l'activité et des statistiques
    /// </summary>
    public class DashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DashboardModel> _logger;

        // Constantes pour éviter les valeurs magiques
        private const string HTTP_CLIENT_NAME = "BasketballAPI";
        private const string TOKEN_SESSION_KEY = "Token";
        private const string USERNAME_SESSION_KEY = "Username";
        private const string ROLE_SESSION_KEY = "Role";
        private const int MAX_RECENT_MATCHES = 10;
        private const int CACHE_DURATION_SECONDS = 30;

        public DashboardModel(IHttpClientFactory httpClientFactory, ILogger<DashboardModel> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Propriétés pour l'affichage
        /// <summary>
        /// Liste des matchs récents
        /// </summary>
        public List<MatchDto> RecentMatches { get; set; } = new List<MatchDto>();

        /// <summary>
        /// Nombre de matchs en cours
        /// </summary>
        public int ActiveMatchesCount { get; set; }

        /// <summary>
        /// Nombre total de matchs
        /// </summary>
        public int TotalMatchesCount { get; set; }

        /// <summary>
        /// Nombre de matchs terminés
        /// </summary>
        public int CompletedMatchesCount { get; set; }

        /// <summary>
        /// Nombre de matchs programmés
        /// </summary>
        public int ScheduledMatchesCount { get; set; }

        /// <summary>
        /// Statistiques globales
        /// </summary>
        public DashboardStatistics Statistics { get; set; } = new DashboardStatistics();

        /// <summary>
        /// Activités récentes
        /// </summary>
        public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();

        /// <summary>
        /// Message d'erreur éventuel
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Message de succès éventuel
        /// </summary>
        [TempData]
        public string SuccessMessage { get; set; } = string.Empty;

        /// <summary>
        /// Chargement des données du tableau de bord
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // Vérification de l'authentification
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Tentative d'accès au dashboard sans authentification");
                return RedirectToPage("/Login");
            }

            var username = HttpContext.Session.GetString(USERNAME_SESSION_KEY);
            var role = HttpContext.Session.GetString(ROLE_SESSION_KEY);

            _logger.LogInformation("Chargement du dashboard pour {Username} ({Role})", username, role);

            try
            {
                // Charger les données depuis l'API
                await LoadMatchesAsync(token);
                await LoadStatisticsAsync(token);
                LoadRecentActivities();

                _logger.LogInformation("Dashboard chargé avec succès : {TotalMatches} matchs, {ActiveMatches} en cours",
                    TotalMatchesCount, ActiveMatchesCount);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur de connexion à l'API lors du chargement du dashboard");
                ErrorMessage = "Impossible de charger les données. Vérifiez la connexion au serveur.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors du chargement du dashboard");
                ErrorMessage = "Une erreur est survenue lors du chargement du tableau de bord.";
            }

            return Page();
        }

        /// <summary>
        /// Charge les matchs depuis l'API
        /// </summary>
        private async Task LoadMatchesAsync(string token)
        {
            try
            {
                var client = CreateAuthenticatedClient(token);
                var response = await client.GetAsync("api/Match");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var allMatches = JsonSerializer.Deserialize<List<MatchDto>>(jsonContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<MatchDto>();

                    // Calcul des statistiques
                    TotalMatchesCount = allMatches.Count;
                    ActiveMatchesCount = allMatches.Count(m => m.Status == "InProgress");
                    CompletedMatchesCount = allMatches.Count(m => m.Status == "Finished");
                    ScheduledMatchesCount = allMatches.Count(m => m.Status == "Scheduled");

                    // Récupération des matchs récents
                    RecentMatches = allMatches
                        .OrderByDescending(m => m.Status == "InProgress" ? 0 : 1) // Matchs en cours en premier
                        .ThenByDescending(m => m.ScheduledDate)
                        .Take(MAX_RECENT_MATCHES)
                        .ToList();

                    _logger.LogDebug("Chargé {Count} matchs depuis l'API", allMatches.Count);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Token expiré ou invalide lors du chargement des matchs");
                    HttpContext.Session.Clear();
                    throw new UnauthorizedAccessException("Session expirée");
                }
                else
                {
                    _logger.LogWarning("Échec du chargement des matchs : {StatusCode}", response.StatusCode);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Timeout lors du chargement des matchs");
                throw new HttpRequestException("La requête a expiré");
            }
        }

        /// <summary>
        /// Charge les statistiques globales
        /// </summary>
        private async Task LoadStatisticsAsync(string token)
        {
            try
            {
                var client = CreateAuthenticatedClient(token);

                // Charger les équipes
                var teamsResponse = await client.GetAsync("api/Team");
                if (teamsResponse.IsSuccessStatusCode)
                {
                    var teamsJson = await teamsResponse.Content.ReadAsStringAsync();
                    var teams = JsonSerializer.Deserialize<List<dynamic>>(teamsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Statistics.TotalTeams = teams?.Count ?? 0;
                }

                // Charger les joueurs
                var playersResponse = await client.GetAsync("api/Player");
                if (playersResponse.IsSuccessStatusCode)
                {
                    var playersJson = await playersResponse.Content.ReadAsStringAsync();
                    var players = JsonSerializer.Deserialize<List<dynamic>>(playersJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Statistics.TotalPlayers = players?.Count ?? 0;
                }

                // Calculer d'autres statistiques
                if (RecentMatches.Any())
                {
                    Statistics.AverageScore = (int)RecentMatches
                        .Where(m => m.Status == "Finished")
                        .SelectMany(m => new[] { m.HomeTeamScore, m.AwayTeamScore })
                        .DefaultIfEmpty(0)
                        .Average();

                    Statistics.TodayMatches = RecentMatches
                        .Count(m => m.ScheduledDate.Date == DateTime.Today);
                }

                _logger.LogDebug("Statistiques chargées : {Teams} équipes, {Players} joueurs",
                    Statistics.TotalTeams, Statistics.TotalPlayers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des statistiques");
                // Ne pas faire échouer toute la page si les stats ne se chargent pas
            }
        }

        /// <summary>
        /// Génère des activités récentes (simulation pour la démo)
        /// </summary>
        private void LoadRecentActivities()
        {
            // Dans une vraie application, ces données viendraient de l'API
            RecentActivities = new List<RecentActivity>
            {
                new RecentActivity
                {
                    Type = "basket",
                    Description = "Panier à 3 points marqué",
                    MatchInfo = "Lakers vs Warriors",
                    TimeAgo = "Il y a 5 minutes",
                    Icon = "bi-trophy",
                    Timestamp = DateTime.Now.AddMinutes(-5)
                },
                new RecentActivity
                {
                    Type = "foul",
                    Description = "Faute personnelle",
                    MatchInfo = "Bulls vs Celtics",
                    TimeAgo = "Il y a 12 minutes",
                    Icon = "bi-exclamation-triangle",
                    Timestamp = DateTime.Now.AddMinutes(-12)
                },
                new RecentActivity
                {
                    Type = "substitution",
                    Description = "Changement de joueur",
                    MatchInfo = "Heat vs Nets",
                    TimeAgo = "Il y a 18 minutes",
                    Icon = "bi-arrow-left-right",
                    Timestamp = DateTime.Now.AddMinutes(-18)
                },
                new RecentActivity
                {
                    Type = "timeout",
                    Description = "Temps mort demandé",
                    MatchInfo = "Spurs vs Rockets",
                    TimeAgo = "Il y a 25 minutes",
                    Icon = "bi-pause-circle",
                    Timestamp = DateTime.Now.AddMinutes(-25)
                }
            };
        }

        /// <summary>
        /// Crée un client HTTP authentifié
        /// </summary>
        private HttpClient CreateAuthenticatedClient(string token)
        {
            var client = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(10); // Timeout court pour le dashboard
            return client;
        }

        /// <summary>
        /// Démarre un match (appelé via AJAX)
        /// </summary>
        public async Task<IActionResult> OnPostStartMatchAsync(int matchId)
        {
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return new JsonResult(new { success = false, message = "Non authentifié" });
            }

            try
            {
                var client = CreateAuthenticatedClient(token);

                // Appeler l'API pour démarrer le match
                var response = await client.PostAsync($"api/LiveScore/match/{matchId}/clock/start", null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Match {MatchId} démarré avec succès", matchId);
                    return new JsonResult(new { success = true, redirectUrl = $"/LiveScore?matchId={matchId}" });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Échec du démarrage du match {MatchId}: {Error}", matchId, error);
                    return new JsonResult(new { success = false, message = "Impossible de démarrer le match" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du démarrage du match {MatchId}", matchId);
                return new JsonResult(new { success = false, message = "Erreur serveur" });
            }
        }

        /// <summary>
        /// Classe pour les statistiques du dashboard
        /// </summary>
        public class DashboardStatistics
        {
            public int TotalTeams { get; set; }
            public int TotalPlayers { get; set; }
            public int AverageScore { get; set; }
            public int TodayMatches { get; set; }
            public double WinRate { get; set; }
            public int TotalActions { get; set; }
        }

        /// <summary>
        /// Classe pour les activités récentes
        /// </summary>
        public class RecentActivity
        {
            public string Type { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string MatchInfo { get; set; } = string.Empty;
            public string TimeAgo { get; set; } = string.Empty;
            public string Icon { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }
    }
}