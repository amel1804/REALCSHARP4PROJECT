using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using BasketballLiveScore.DTOs;

namespace BasketballLiveScore.Pages
{
    /// <summary>
    /// Page de connexion - Gère l'authentification des utilisateurs
    /// Utilise HttpClient comme dans les exemples de cours
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        // Constantes pour éviter les valeurs magiques
        private const string HTTP_CLIENT_NAME = "BasketballAPI";
        private const string TOKEN_SESSION_KEY = "Token";
        private const string USERNAME_SESSION_KEY = "Username";
        private const string ROLE_SESSION_KEY = "Role";

        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Modèle de liaison pour le formulaire de connexion
        /// </summary>
        [BindProperty]
        public LoginInputModel LoginInput { get; set; } = new LoginInputModel();

        /// <summary>
        /// Message d'erreur à afficher
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Classe interne pour les données du formulaire - Pattern vu en cours
        /// </summary>
        public class LoginInputModel
        {
            [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
            [Display(Name = "Nom d'utilisateur")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le mot de passe est obligatoire")]
            [DataType(DataType.Password)]
            [Display(Name = "Mot de passe")]
            public string Password { get; set; } = string.Empty;
        }

        /// <summary>
        /// Gestion du GET - Affichage du formulaire
        /// </summary>
        public IActionResult OnGet()
        {
            // Si déjà connecté, rediriger vers le tableau de bord
            if (HttpContext.Session.GetString(TOKEN_SESSION_KEY) != null)
            {
                return RedirectToPage("/Dashboard");
            }

            return Page();
        }

        /// <summary>
        /// Gestion du POST - Traitement de la connexion
        /// Pattern async/await comme vu dans les cours
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // Validation du modèle
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Création du client HTTP - Pattern vu dans les notes HttpClient
                var client = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);

                // Préparation des données de connexion
                var loginDto = new UserConnectionDto
                {
                    Name = LoginInput.Name,
                    Password = LoginInput.Password
                };

                // Sérialisation en JSON
                var jsonContent = JsonSerializer.Serialize(loginDto);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Appel à l'API
                var response = await client.PostAsync("api/Authentication/Login", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    // Lecture de la réponse
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
                    {
                        // Stockage du token et des informations utilisateur en session
                        HttpContext.Session.SetString(TOKEN_SESSION_KEY, tokenResponse.Token);
                        HttpContext.Session.SetString(USERNAME_SESSION_KEY, LoginInput.Name);

                        // Extraction du rôle depuis le token (si nécessaire)
                        // Pour simplifier, on stocke un rôle par défaut
                        HttpContext.Session.SetString(ROLE_SESSION_KEY, "Encoder");

                        return RedirectToPage("/Dashboard");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect";
                }
                else
                {
                    ErrorMessage = "Une erreur est survenue lors de la connexion";
                }
            }
            catch (HttpRequestException ex)
            {
                // Log de l'erreur en production
                ErrorMessage = $"Impossible de contacter le serveur : {ex.Message}";
            }
            catch (Exception ex)
            {
                // Log de l'erreur en production
                ErrorMessage = $"Une erreur inattendue est survenue : {ex.Message}";
            }

            return Page();
        }

        /// <summary>
        /// Classe pour désérialiser la réponse du token
        /// </summary>
        private class TokenResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}