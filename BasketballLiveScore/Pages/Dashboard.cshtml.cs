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
    /// Affiche un r�sum� complet de l'activit� et des statistiques
    /// </summary>
    public class DashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DashboardModel> _logger;

        // Constantes pour �viter les valeurs magiques
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

        // Propri�t�s pour l'affichage
        /// <summary>
        /// Liste des matchs r�cents
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
        /// Nombre de matchs termin�s
        /// </summary>
        public int CompletedMatchesCount { get; set; }

        /// <summary>
        /// Nombre de matchs programm�s
        /// </summary>
        public int ScheduledMatchesCount { get; set; }

        /// <summary>
        /// Statistiques globales
        /// </summary>
        public DashboardStatistics Statistics { get; set; } = new DashboardStatistics();

        /// <summary>
        /// Activit�s r�centes
        /// </summary>
        public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();

        /// <summary>
        /// Message d'erreur �ventuel
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Message de succ�s �ventuel
        /// </summary>
        [TempData]
        public string SuccessMessage { get; set; } = string.Empty;

        /// <summary>
        /// Chargement des donn�es du tableau de bord
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // V�rification de l'authentification
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Tentative d'acc�s au dashboard sans authentification");
                return RedirectToPage("/Login");
            }

            var username = HttpContext.Session.GetString(USERNAME_SESSION_KEY);
            var role = HttpContext.Session.GetString(ROLE_SESSION_KEY);

            _logger.LogInformation("Chargement du dashboard pour {Username} ({Role})", username, role);

            try
            {
                // Charger les donn�es depuis l'API
                await LoadMatchesAsync(token);
                await LoadStatisticsAsync(token);
                LoadRecentActivities();

                _logger.LogInformation("Dashboard charg� avec succ�s : {TotalMatches} matchs, {ActiveMatches} en cours",
                    TotalMatchesCount, ActiveMatchesCount);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur de connexion � l'API lors du chargement du dashboard");
                ErrorMessage = "Impossible de charger les donn�es. V�rifiez la connexion au serveur.";
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

                    // R�cup�ration des matchs r�cents
                    RecentMatches = allMatches
                        .OrderByDescending(m => m.Status == "InProgress" ? 0 : 1) // Matchs en cours en premier
                        .ThenByDescending(m => m.ScheduledDate)
                        .Take(MAX_RECENT_MATCHES)
                        .ToList();

                    _logger.LogDebug("Charg� {Count} matchs depuis l'API", allMatches.Count);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Token expir� ou invalide lors du chargement des matchs");
                    HttpContext.Session.Clear();
                    throw new UnauthorizedAccessException("Session expir�e");
                }
                else
                {
                    _logger.LogWarning("�chec du chargement des matchs : {StatusCode}", response.StatusCode);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Timeout lors du chargement des matchs");
                throw new HttpRequestException("La requ�te a expir�");
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

                // Charger les �quipes
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

                _logger.LogDebug("Statistiques charg�es : {Teams} �quipes, {Players} joueurs",
                    Statistics.TotalTeams, Statistics.TotalPlayers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des statistiques");
                // Ne pas faire �chouer toute la page si les stats ne se chargent pas
            }
        }

        /// <summary>
        /// G�n�re des activit�s r�centes (simulation pour la d�mo)
        /// </summary>
        private void LoadRecentActivities()
        {
            // Dans une vraie application, ces donn�es viendraient de l'API
            RecentActivities = new List<RecentActivity>
            {
                new RecentActivity
                {
                    Type = "basket",
                    Description = "Panier � 3 points marqu�",
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
                    Description = "Temps mort demand�",
                    MatchInfo = "Spurs vs Rockets",
                    TimeAgo = "Il y a 25 minutes",
                    Icon = "bi-pause-circle",
                    Timestamp = DateTime.Now.AddMinutes(-25)
                }
            };
        }

        /// <summary>
        /// Cr�e un client HTTP authentifi�
        /// </summary>
        private HttpClient CreateAuthenticatedClient(string token)
        {
            var client = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(10); // Timeout court pour le dashboard
            return client;
        }

        /// <summary>
        /// D�marre un match (appel� via AJAX)
        /// </summary>
        public async Task<IActionResult> OnPostStartMatchAsync(int matchId)
        {
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return new JsonResult(new { success = false, message = "Non authentifi�" });
            }

            try
            {
                var client = CreateAuthenticatedClient(token);

                // Appeler l'API pour d�marrer le match
                var response = await client.PostAsync($"api/LiveScore/match/{matchId}/clock/start", null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Match {MatchId} d�marr� avec succ�s", matchId);
                    return new JsonResult(new { success = true, redirectUrl = $"/LiveScore?matchId={matchId}" });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("�chec du d�marrage du match {MatchId}: {Error}", matchId, error);
                    return new JsonResult(new { success = false, message = "Impossible de d�marrer le match" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du d�marrage du match {MatchId}", matchId);
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
        /// Classe pour les activit�s r�centes
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