using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BasketballLiveScore.DTOs.Team;

namespace BasketballLiveScore.Pages
{
    /// <summary>
    /// Page de gestion des équipes
    /// Permet la création, modification et suppression d'équipes
    /// </summary>
    public class TeamsModel : PageModel
    {
        // Constantes pour éviter les valeurs magiques
        private const string HTTP_CLIENT_NAME = "BasketballAPI";
        private const string TOKEN_SESSION_KEY = "Token";
        private const string API_ENDPOINT = "api/Team";
        private const int REFRESH_DELAY_SECONDS = 2;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TeamsModel> _logger;

        public TeamsModel(IHttpClientFactory httpClientFactory, ILogger<TeamsModel> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Liste des équipes à afficher
        /// </summary>
        public List<TeamDto> Teams { get; set; } = new();

        /// <summary>
        /// Modèle pour créer une nouvelle équipe
        /// </summary>
        [BindProperty]
        public CreateTeamDto NewTeam { get; set; } = new();

        /// <summary>
        /// Modèle pour modifier une équipe
        /// </summary>
        [BindProperty]
        public UpdateTeamInputModel UpdateTeam { get; set; } = new();

        /// <summary>
        /// Message de succès à afficher
        /// </summary>
        [TempData]
        public string SuccessMessage { get; set; } = string.Empty;

        /// <summary>
        /// Message d'erreur à afficher
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Classe interne pour la mise à jour d'équipe
        /// </summary>
        public class UpdateTeamInputModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Coach { get; set; } = string.Empty;
        }

        /// <summary>
        /// Chargement de la page - Récupération de toutes les équipes
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // Vérification de l'authentification
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            await LoadTeamsAsync(token);
            return Page();
        }

        /// <summary>
        /// Création d'une nouvelle équipe
        /// </summary>
        public async Task<IActionResult> OnPostCreateAsync()
        {
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                await LoadTeamsAsync(token);
                return Page();
            }

            try
            {
                var client = CreateAuthenticatedClient(token);

                var jsonContent = JsonSerializer.Serialize(NewTeam);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(API_ENDPOINT, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = $"L'équipe '{NewTeam.Name}' a été créée avec succès.";
                    _logger.LogInformation("Équipe créée: {TeamName}", NewTeam.Name);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Erreur lors de la création de l'équipe: {error}";
                    _logger.LogError("Erreur création équipe: {Error}", error);
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = "Impossible de contacter le serveur.";
                _logger.LogError(ex, "Erreur HTTP lors de la création d'équipe");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Une erreur inattendue est survenue.";
                _logger.LogError(ex, "Erreur inattendue lors de la création d'équipe");
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Mise à jour d'une équipe existante
        /// </summary>
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var client = CreateAuthenticatedClient(token);

                var updateDto = new UpdateTeamDto
                {
                    Name = UpdateTeam.Name,
                    City = UpdateTeam.City,
                    Coach = UpdateTeam.Coach
                };

                var jsonContent = JsonSerializer.Serialize(updateDto);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{API_ENDPOINT}/{UpdateTeam.Id}", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "L'équipe a été modifiée avec succès.";
                    _logger.LogInformation("Équipe modifiée: ID {TeamId}", UpdateTeam.Id);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Erreur lors de la modification: {error}";
                    _logger.LogError("Erreur modification équipe {TeamId}: {Error}", UpdateTeam.Id, error);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Une erreur est survenue lors de la modification.";
                _logger.LogError(ex, "Erreur lors de la modification de l'équipe {TeamId}", UpdateTeam.Id);
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Suppression d'une équipe
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var client = CreateAuthenticatedClient(token);
                var response = await client.DeleteAsync($"{API_ENDPOINT}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "L'équipe a été supprimée avec succès.";
                    _logger.LogInformation("Équipe supprimée: ID {TeamId}", id);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ErrorMessage = "L'équipe n'a pas été trouvée.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Erreur lors de la suppression: {error}";
                    _logger.LogError("Erreur suppression équipe {TeamId}: {Error}", id, error);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Une erreur est survenue lors de la suppression.";
                _logger.LogError(ex, "Erreur lors de la suppression de l'équipe {TeamId}", id);
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Méthode privée pour charger la liste des équipes
        /// </summary>
        private async Task LoadTeamsAsync(string token)
        {
            try
            {
                var client = CreateAuthenticatedClient(token);
                var response = await client.GetAsync(API_ENDPOINT);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Teams = JsonSerializer.Deserialize<List<TeamDto>>(jsonContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<TeamDto>();

                    _logger.LogInformation("Chargement de {Count} équipes", Teams.Count);
                }
                else
                {
                    _logger.LogWarning("Échec du chargement des équipes: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des équipes");
                Teams = new List<TeamDto>();
            }
        }

        /// <summary>
        /// Crée un client HTTP authentifié
        /// </summary>
        private HttpClient CreateAuthenticatedClient(string token)
        {
            var client = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}