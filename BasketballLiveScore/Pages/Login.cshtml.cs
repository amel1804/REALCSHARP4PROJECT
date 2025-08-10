using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BasketballLiveScore.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILoginService _loginService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ILoginService loginService, IConfiguration configuration, ILogger<LoginModel> logger)
        {
            _loginService = loginService;
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty]
        public LoginInputModel LoginInput { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public class LoginInputModel
        {
            [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
            [Display(Name = "Nom d'utilisateur")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le mot de passe est obligatoire")]
            [DataType(DataType.Password)]
            [Display(Name = "Mot de passe")]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            // Si déjà connecté, rediriger vers le dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))
            {
                Response.Redirect("/Dashboard");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Authentification
                var user = await Task.Run(() =>
                    _loginService.Login(LoginInput.Username, LoginInput.Password));

                if (user == null)
                {
                    ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect";
                    _logger.LogWarning("Échec de connexion pour {Username}", LoginInput.Username);
                    return Page();
                }

                // Génération du token JWT
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    jwtKey = "CleDeSecuriteParDefautPourBasketballLiveScore2024!";
                }

                var token = _loginService.GenerateToken(jwtKey, claims);

                // Stockage dans la session
                HttpContext.Session.SetString("Token", token);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("UserId", user.Id.ToString());

                _logger.LogInformation("Connexion réussie pour {Username}", user.Username);

                return RedirectToPage("/Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion");
                ErrorMessage = "Une erreur est survenue lors de la connexion";
                return Page();
            }
        }
    }
}