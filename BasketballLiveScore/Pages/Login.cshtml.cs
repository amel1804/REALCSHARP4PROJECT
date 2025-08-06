// Pages/Login.cshtml.cs
using BasketballLiveScore.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BasketballLiveScore.Pages
{
    /// <summary>
    /// Page de connexion utilisant l'authentification JWT
    /// Basé sur le pattern vu dans les notes de cours sur Authorization with JWT
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        // Propriétés pour le binding du formulaire
        [BindProperty]
        public UserConnectionDto LoginInput { get; set; } = new UserConnectionDto();

        // Message d'erreur à afficher
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Constructeur avec injection de dépendances
        /// Suit le pattern vu dans les codes de cours DependencyInjection
        /// </summary>
        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Affichage de la page de login
        /// </summary>
        public void OnGet()
        {
            // Réinitialiser le formulaire
            LoginInput = new UserConnectionDto();
        }

        /// <summary>
        /// Traitement de la soumission du formulaire de connexion
        /// Communique avec l'API pour obtenir le JWT token
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
                // Création du client HTTP via la factory (pattern vu en cours)
                var httpClient = _httpClientFactory.CreateClient("BasketballAPI");

                // Sérialisation du DTO en JSON
                var jsonContent = JsonSerializer.Serialize(LoginInput);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Appel à l'API d'authentification
                var response = await httpClient.PostAsync("/api/Authentication/Login", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    // Lecture de la réponse contenant le token
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
                    {
                        // Stockage du token dans la session
                        HttpContext.Session.SetString("JWTToken", tokenResponse.Token);
                        HttpContext.Session.SetString("Username", LoginInput.Name);

                        // Redirection vers le dashboard
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
                ErrorMessage = "Impossible de contacter le serveur";
            }
            catch (Exception ex)
            {
                // Log de l'erreur en production
                ErrorMessage = "Une erreur inattendue est survenue";
            }

            return Page();
        }

        /// <summary>
        /// Classe interne pour désérialiser la réponse du token
        /// </summary>
        private class TokenResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}