using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using BasketballLiveScore.DTOs.User;
using Microsoft.Extensions.Logging;

namespace BasketballLiveScore.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginModel> _logger;

        // Constantes pour éviter les valeurs magiques
        private const string HTTP_CLIENT_NAME = "BasketballAPI";
        private const string TOKEN_SESSION_KEY = "Token";
        private const string USERNAME_SESSION_KEY = "Username";
        private const string ROLE_SESSION_KEY = "Role";
        private const string USER_ID_SESSION_KEY = "UserId";
        private const int SESSION_TIMEOUT_HOURS = 8;

        public LoginModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<LoginModel> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [BindProperty]
        public LoginInputModel LoginInput { get; set; } = new LoginInputModel();

        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        [TempData]
        public string SuccessMessage { get; set; } = string.Empty;

        public string ReturnUrl { get; set; } = "/Dashboard";

        public class LoginInputModel
        {
            [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
            [Display(Name = "Nom d'utilisateur")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le mot de passe est obligatoire")]
            [DataType(DataType.Password)]
            [Display(Name = "Mot de passe")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Se souvenir de moi")]
            public bool RememberMe { get; set; }
        }

        public IActionResult OnGet(string returnUrl = null)
        {
            // Si déjà connecté, rediriger
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(TOKEN_SESSION_KEY)))
            {
                _logger.LogInformation("Utilisateur déjà connecté, redirection vers le dashboard");
                return RedirectToPage("/Dashboard");
            }

            ReturnUrl = returnUrl ?? "/Dashboard";

            // Vérifier si on vient de la page d'inscription
            if (TempData.ContainsKey("SuccessMessage"))
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString() ?? string.Empty;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? "/Dashboard";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);

                // Construire l'URL de l'API
                var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }

                var loginDto = new UserLoginDto
                {
                    Username = LoginInput.Username,
                    Password = LoginInput.Password
                };

                var jsonContent = JsonSerializer.Serialize(loginDto);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Tentative de connexion pour l'utilisateur: {Username}", LoginInput.Username);

                var response = await client.PostAsync($"{baseUrl}api/Authentication/Login", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("Réponse reçue: {StatusCode}", response.StatusCode);

                    try
                    {
                        using var document = JsonDocument.Parse(responseContent);
                        var root = document.RootElement;

                        // Récupérer le token
                        if (root.TryGetProperty("token", out var tokenElement) ||
                            root.TryGetProperty("Token", out tokenElement))
                        {
                            var token = tokenElement.GetString();

                            if (!string.IsNullOrEmpty(token))
                            {
                                // Stocker les informations dans la session
                                HttpContext.Session.SetString(TOKEN_SESSION_KEY, token);
                                HttpContext.Session.SetString(USERNAME_SESSION_KEY, LoginInput.Username);

                                // Récupérer le rôle depuis la réponse
                                if (root.TryGetProperty("role", out var roleElement) ||
                                    root.TryGetProperty("Role", out roleElement))
                                {
                                    var role = roleElement.GetString() ?? "Viewer";
                                    HttpContext.Session.SetString(ROLE_SESSION_KEY, role);
                                    _logger.LogInformation("Rôle de l'utilisateur: {Role}", role);
                                }

                                // Récupérer l'ID utilisateur si disponible
                                if (root.TryGetProperty("userId", out var userIdElement) ||
                                    root.TryGetProperty("UserId", out userIdElement))
                                {
                                    var userId = userIdElement.GetString();
                                    if (!string.IsNullOrEmpty(userId))
                                    {
                                        HttpContext.Session.SetString(USER_ID_SESSION_KEY, userId);
                                    }
                                }

                                _logger.LogInformation("Connexion réussie pour: {Username}", LoginInput.Username);

                                // Si "Se souvenir de moi" est coché, étendre la durée du cookie
                                if (LoginInput.RememberMe)
                                {
                                    HttpContext.Session.SetString("RememberMe", "true");
                                }

                                // Redirection selon le rôle
                                return RedirectToPage(ReturnUrl);
                            }
                            else
                            {
                                _logger.LogWarning("Token vide reçu pour: {Username}", LoginInput.Username);
                                ErrorMessage = "Erreur lors de la connexion : token invalide";
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Token non trouvé dans la réponse pour: {Username}", LoginInput.Username);
                            ErrorMessage = "Erreur lors de la connexion : réponse invalide";
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Erreur lors du parsing JSON pour: {Username}", LoginInput.Username);
                        ErrorMessage = "Erreur lors de la connexion : format de réponse invalide";
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Échec de connexion (401) pour: {Username}", LoginInput.Username);
                    ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Erreur API ({StatusCode}): {Error}", response.StatusCode, errorContent);
                    ErrorMessage = $"Erreur lors de la connexion (Code: {response.StatusCode})";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur HTTP lors de la connexion");
                ErrorMessage = "Impossible de contacter le serveur. Vérifiez que l'API est démarrée.";
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout lors de la connexion");
                ErrorMessage = "La connexion au serveur a expiré. Veuillez réessayer.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de la connexion");
                ErrorMessage = "Une erreur inattendue est survenue. Veuillez réessayer.";
            }

            return Page();
        }

        public IActionResult OnPostQuickLogin(string username, string password)
        {
            LoginInput.Username = username;
            LoginInput.Password = password;
            return OnPostAsync().Result;
        }
    }
}