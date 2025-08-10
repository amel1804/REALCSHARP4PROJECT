using BasketballLiveScore.DTOs.Match;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace BasketballLiveScore.Pages
{
    /// <summary>
    /// Page d'encodage en temps r�el des scores
    /// Permet l'enregistrement de toutes les actions du match
    /// </summary>
    public class LiveScoreModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Constantes
        private const string HTTP_CLIENT_NAME = "BasketballAPI";
        private const string TOKEN_SESSION_KEY = "Token";

        public LiveScoreModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <summary>
        /// Match actuellement en cours d'encodage
        /// </summary>
        public MatchDto? CurrentMatch { get; set; }

        /// <summary>
        /// Identifiant du match (depuis la query string)
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int? MatchId { get; set; }

        /// <summary>
        /// Indique si on doit d�marrer le match
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public bool Start { get; set; }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // V�rification de l'authentification
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            // Si pas d'ID de match, retour au dashboard
            if (!MatchId.HasValue)
            {
                return RedirectToPage("/Dashboard");
            }

            try
            {
                // Configuration du client HTTP
                var client = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // R�cup�ration du match
                var response = await client.GetAsync($"api/Match/{MatchId.Value}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    CurrentMatch = JsonSerializer.Deserialize<MatchDto>(jsonContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Si on doit d�marrer le match
                    if (Start && CurrentMatch != null && CurrentMatch.Status == "Scheduled")
                    {
                        await StartMatch();
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Match non trouv�
                    return RedirectToPage("/Dashboard");
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

        /// <summary>
        /// D�marre le match (changement de statut)
        /// </summary>
        private async Task StartMatch()
        {
            if (CurrentMatch == null) return;

            try
            {
                var client = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
                var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Mise � jour du statut du match
                CurrentMatch.Status = "InProgress";
                CurrentMatch.CurrentQuarter = 1;

                var jsonContent = JsonSerializer.Serialize(CurrentMatch);
                var httpContent = new StringContent(jsonContent,
                    System.Text.Encoding.UTF8, "application/json");

                // Appel API pour mettre � jour le match
                // Note: Vous devrez peut-�tre ajouter un endpoint PUT dans votre MatchController
                var response = await client.PutAsync($"api/Match/{CurrentMatch.Id}", httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty,
                        $"Erreur lors du d�marrage du match : {error}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Erreur lors du d�marrage du match : {ex.Message}");
            }
        }
    }
}