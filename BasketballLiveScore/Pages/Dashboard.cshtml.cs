using BasketballLiveScore.DTOs.Match;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace BasketballLiveScore.Pages
{
    /// <summary>
    /// Page du tableau de bord principal
    /// Affiche un résumé des matchs et des actions rapides
    /// </summary>
    public class DashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Constantes pour éviter les valeurs magiques
        private const string HTTP_CLIENT_NAME = "BasketballAPI";
        private const string TOKEN_SESSION_KEY = "Token";
        private const int MAX_RECENT_MATCHES = 10;

        public DashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

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
        /// Chargement des données du tableau de bord
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // Vérification de l'authentification
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // Configuration du client HTTP avec le token JWT
                var client = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Récupération de tous les matchs
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

                    // Récupération des matchs récents (10 derniers)
                    RecentMatches = allMatches
                        .OrderByDescending(m => m.ScheduledDate)
                        .Take(MAX_RECENT_MATCHES)
                        .ToList();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Token expiré ou invalide
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Login");
                }
            }
            catch (HttpRequestException ex)
            {
                // Log de l'erreur
                ModelState.AddModelError(string.Empty,
                    $"Erreur de connexion au serveur : {ex.Message}");
            }

            return Page();
        }
    }
}