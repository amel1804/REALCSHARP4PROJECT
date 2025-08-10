using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BasketballLiveScore.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Récupérer le nom d'utilisateur avant de nettoyer la session
            var username = HttpContext.Session.GetString("Username");

            // Nettoyer toute la session
            HttpContext.Session.Clear();

            // Logger la déconnexion
            if (!string.IsNullOrEmpty(username))
            {
                _logger.LogInformation("Utilisateur {Username} déconnecté", username);
            }

            // Ajouter un message de confirmation
            TempData["SuccessMessage"] = "Vous avez été déconnecté avec succès.";

            // Rediriger vers la page de connexion
            return RedirectToPage("/Login");
        }

        public IActionResult OnPost()
        {
            return OnGet();
        }
    }
}