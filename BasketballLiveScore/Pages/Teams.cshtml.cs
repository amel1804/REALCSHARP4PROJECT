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
    /// Page de gestion des �quipes
    /// Permet la cr�ation, modification et suppression d'�quipes
    /// </summary>
    public class TeamsModel : PageModel
    {
        // Constantes pour �viter les valeurs magiques
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
        /// Liste des �quipes � afficher
        /// </summary>
        public List<TeamDto> Teams { get; set; } = new();

        /// <summary>
        /// Mod�le pour cr�er une nouvelle �quipe
        /// </summary>
        [BindProperty]
        public CreateTeamDto NewTeam { get; set; } = new();

        /// <summary>
        /// Mod�le pour modifier une �quipe
        /// </summary>
        [BindProperty]
        public UpdateTeamInputModel UpdateTeam { get; set; } = new();

        /// <summary>
        /// Message de succ�s � afficher
        /// </summary>
        [TempData]
        public string SuccessMessage { get; set; } = string.Empty;

        /// <summary>
        /// Message d'erreur � afficher
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Classe interne pour la mise � jour d'�quipe
        /// </summary>
        public class UpdateTeamInputModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Coach { get; set; } = string.Empty;
        }

        /// <summary>
        /// Chargement de la page - R�cup�ration de toutes les �quipes
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // V�rification de l'authentification
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            await LoadTeamsAsync(token);
            return Page();
        }

        /// <summary>
        /// Cr�ation d'une nouvelle �quipe
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
                    SuccessMessage = $"L'�quipe '{NewTeam.Name}' a �t� cr��e avec succ�s.";
                    _logger.LogInformation("�quipe cr��e: {TeamName}", NewTeam.Name);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Erreur lors de la cr�ation de l'�quipe: {error}";
                    _logger.LogError("Erreur cr�ation �quipe: {Error}", error);
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = "Impossible de contacter le serveur.";
                _logger.LogError(ex, "Erreur HTTP lors de la cr�ation d'�quipe");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Une erreur inattendue est survenue.";
                _logger.LogError(ex, "Erreur inattendue lors de la cr�ation d'�quipe");
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Mise � jour d'une �quipe existante
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
                    SuccessMessage = "L'�quipe a �t� modifi�e avec succ�s.";
                    _logger.LogInformation("�quipe modifi�e: ID {TeamId}", UpdateTeam.Id);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Erreur lors de la modification: {error}";
                    _logger.LogError("Erreur modification �quipe {TeamId}: {Error}", UpdateTeam.Id, error);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Une erreur est survenue lors de la modification.";
                _logger.LogError(ex, "Erreur lors de la modification de l'�quipe {TeamId}", UpdateTeam.Id);
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Suppression d'une �quipe
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
                    SuccessMessage = "L'�quipe a �t� supprim�e avec succ�s.";
                    _logger.LogInformation("�quipe supprim�e: ID {TeamId}", id);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ErrorMessage = "L'�quipe n'a pas �t� trouv�e.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Erreur lors de la suppression: {error}";
                    _logger.LogError("Erreur suppression �quipe {TeamId}: {Error}", id, error);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Une erreur est survenue lors de la suppression.";
                _logger.LogError(ex, "Erreur lors de la suppression de l'�quipe {TeamId}", id);
            }

            return RedirectToPage();
        }

        /// <summary>
        /// M�thode priv�e pour charger la liste des �quipes
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

                    _logger.LogInformation("Chargement de {Count} �quipes", Teams.Count);
                }
                else
                {
                    _logger.LogWarning("�chec du chargement des �quipes: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des �quipes");
                Teams = new List<TeamDto>();
            }
        }

        /// <summary>
        /// Cr�e un client HTTP authentifi�
        /// </summary>
        private HttpClient CreateAuthenticatedClient(string token)
        {
            var client = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}