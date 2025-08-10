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
            // R�cup�rer le nom d'utilisateur avant de nettoyer la session
            var username = HttpContext.Session.GetString("Username");

            // Nettoyer toute la session
            HttpContext.Session.Clear();

            // Logger la d�connexion
            if (!string.IsNullOrEmpty(username))
            {
                _logger.LogInformation("Utilisateur {Username} d�connect�", username);
            }

            // Ajouter un message de confirmation
            TempData["SuccessMessage"] = "Vous avez �t� d�connect� avec succ�s.";

            // Rediriger vers la page de connexion
            return RedirectToPage("/Login");
        }

        public IActionResult OnPost()
        {
            return OnGet();
        }
    }
}