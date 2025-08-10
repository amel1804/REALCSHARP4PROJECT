using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BasketballLiveScore.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IRegisterService _registerService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IRegisterService registerService, ILogger<RegisterModel> logger)
        {
            _registerService = registerService;
            _logger = logger;
        }

        [BindProperty]
        public RegisterInputModel RegisterInput { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public class RegisterInputModel
        {
            [Required(ErrorMessage = "Le prénom est obligatoire")]
            [Display(Name = "Prénom")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le nom est obligatoire")]
            [Display(Name = "Nom")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
            [Display(Name = "Nom d'utilisateur")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "L'email est obligatoire")]
            [EmailAddress(ErrorMessage = "Format d'email invalide")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le mot de passe est obligatoire")]
            [StringLength(100, ErrorMessage = "Le {0} doit contenir au moins {2} caractères.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mot de passe")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmer le mot de passe")]
            [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le rôle est obligatoire")]
            [Display(Name = "Rôle")]
            public string Role { get; set; } = "Viewer";
        }

        public void OnGet()
        {
            // Page GET - rien à faire
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Pour l'instant, on utilise le username comme base pour créer l'utilisateur
                // Le service RegisterService attend (username, password, role)
                // On devra adapter le service pour accepter toutes les données

                var result = await Task.Run(() =>
                    _registerService.Register(RegisterInput.Username, RegisterInput.Password, RegisterInput.Role));

                if (result == "OK")
                {
                    _logger.LogInformation("Nouvel utilisateur inscrit: {Username}", RegisterInput.Username);
                    SuccessMessage = "Inscription réussie ! Vous pouvez maintenant vous connecter.";

                    // Redirection vers la page de connexion après 2 secondes
                    return RedirectToPage("/Login");
                }
                else
                {
                    ErrorMessage = result;
                    _logger.LogWarning("Échec de l'inscription pour {Username}: {Error}", RegisterInput.Username, result);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription");
                ErrorMessage = "Une erreur est survenue lors de l'inscription. Veuillez réessayer.";
                return Page();
            }
        }
    }
}