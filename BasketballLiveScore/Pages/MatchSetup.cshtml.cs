using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BasketballLiveScore.DTOs.Match;

namespace BasketballLiveScore.Pages
{
    /// <summary>
    /// Page de configuration d'un nouveau match
    /// Permet de d�finir tous les param�tres du match avant son d�marrage
    /// </summary>
    public class MatchSetupModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Constantes
        private const string HTTP_CLIENT_NAME = "BasketballAPI";
        private const string TOKEN_SESSION_KEY = "Token";
        private const int DEFAULT_QUARTER_DURATION = 10;
        private const int DEFAULT_TIMEOUT_DURATION = 60;
        private const int DEFAULT_NUMBER_OF_QUARTERS = 4;
        private const int PLAYERS_PER_STARTING_LINEUP = 5;

        public MatchSetupModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <summary>
        /// Mod�le de liaison pour le formulaire
        /// </summary>
        [BindProperty]
        public MatchSetupInputModel MatchSetup { get; set; } = new MatchSetupInputModel();

        /// <summary>
        /// Message de succ�s
        /// </summary>
        public string SuccessMessage { get; set; } = string.Empty;

        /// <summary>
        /// Listes pour les s�lections (si n�cessaire)
        /// </summary>
        public List<PlayerInputModel> HomeStarters { get; set; } = new List<PlayerInputModel>();
        public List<PlayerInputModel> AwayStarters { get; set; } = new List<PlayerInputModel>();

        /// <summary>
        /// Classe pour les donn�es du formulaire
        /// </summary>
        public class MatchSetupInputModel
        {
            [Required(ErrorMessage = "La date du match est obligatoire")]
            [Display(Name = "Date et heure")]
            public DateTime ScheduledDate { get; set; } = DateTime.Now.AddDays(1);

            [Display(Name = "Lieu")]
            [MaxLength(100)]
            public string Location { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le nombre de quart-temps est obligatoire")]
            [Range(2, 4, ErrorMessage = "Le nombre de quart-temps doit �tre entre 2 et 4")]
            [Display(Name = "Nombre de quart-temps")]
            public int NumberOfQuarters { get; set; } = DEFAULT_NUMBER_OF_QUARTERS;

            [Required(ErrorMessage = "La dur�e des quart-temps est obligatoire")]
            [Range(5, 20, ErrorMessage = "La dur�e doit �tre entre 5 et 20 minutes")]
            [Display(Name = "Dur�e d'un quart-temps (minutes)")]
            public int QuarterDurationMinutes { get; set; } = DEFAULT_QUARTER_DURATION;

            [Required(ErrorMessage = "La dur�e des timeouts est obligatoire")]
            [Range(30, 120, ErrorMessage = "La dur�e doit �tre entre 30 et 120 secondes")]
            [Display(Name = "Dur�e d'un timeout (secondes)")]
            public int TimeoutDurationSeconds { get; set; } = DEFAULT_TIMEOUT_DURATION;

            [Required(ErrorMessage = "L'�quipe domicile est obligatoire")]
            [Display(Name = "�quipe domicile")]
            [MaxLength(50)]
            public string HomeTeamName { get; set; } = string.Empty;

            [Required(ErrorMessage = "L'�quipe visiteur est obligatoire")]
            [Display(Name = "�quipe visiteur")]
            [MaxLength(50)]
            public string AwayTeamName { get; set; } = string.Empty;
        }

        /// <summary>
        /// Classe pour les joueurs
        /// </summary>
        public class PlayerInputModel
        {
            public string Name { get; set; } = string.Empty;
            public int Number { get; set; }
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        public IActionResult OnGet()
        {
            // V�rification de l'authentification
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            // Initialisation des listes de joueurs
            InitializePlayerLists();

            return Page();
        }

        /// <summary>
        /// Traitement du formulaire de cr�ation de match
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // V�rification de l'authentification
            var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                InitializePlayerLists();
                return Page();
            }

            try
            {
                // Configuration du client HTTP
                var client = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Cr�ation du DTO pour l'API
                var matchDto = new MatchDto
                {
                    ScheduledDate = MatchSetup.ScheduledDate,
                    HomeTeamName = MatchSetup.HomeTeamName,
                    AwayTeamName = MatchSetup.AwayTeamName,
                    Status = "Scheduled",
                    CurrentQuarter = 0,
                    HomeTeamScore = 0,
                    AwayTeamScore = 0
                };

                // S�rialisation et envoi
                var jsonContent = JsonSerializer.Serialize(matchDto);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Match", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Match cr�� avec succ�s !";

                    // R�initialisation du formulaire
                    MatchSetup = new MatchSetupInputModel();
                    InitializePlayerLists();

                    // Redirection apr�s 2 secondes
                    Response.Headers.Add("Refresh", "2; url=/Dashboard");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty,
                        $"Erreur lors de la cr�ation du match : {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Erreur de connexion au serveur : {ex.Message}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Une erreur inattendue est survenue : {ex.Message}");
            }

            InitializePlayerLists();
            return Page();
        }

        /// <summary>
        /// Initialise les listes de joueurs vides
        /// </summary>
        private void InitializePlayerLists()
        {
            if (HomeStarters.Count == 0)
            {
                for (int i = 0; i < PLAYERS_PER_STARTING_LINEUP; i++)
                {
                    HomeStarters.Add(new PlayerInputModel());
                }
            }

            if (AwayStarters.Count == 0)
            {
                for (int i = 0; i < PLAYERS_PER_STARTING_LINEUP; i++)
                {
                    AwayStarters.Add(new PlayerInputModel());
                }
            }
        }
    }
}