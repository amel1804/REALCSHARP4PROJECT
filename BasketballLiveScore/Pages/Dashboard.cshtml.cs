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
    /// Affiche un r�sum� des matchs et des actions rapides
    /// </summary>
    public class DashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Constantes pour �viter les valeurs magiques
        private const string HTTP_CLIENT_NAME = "BasketballAPI";
        private const string TOKEN_SESSION_KEY = "Token";
        private const int MAX_RECENT_MATCHES = 10;

        public DashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

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
        /// Chargement des donn�es du tableau de bord
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // V�rification de l'authentification
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

                // R�cup�ration de tous les matchs
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

                    // R�cup�ration des matchs r�cents (10 derniers)
                    RecentMatches = allMatches
                        .OrderByDescending(m => m.ScheduledDate)
                        .Take(MAX_RECENT_MATCHES)
                        .ToList();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Token expir� ou invalide
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