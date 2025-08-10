using BasketballLiveScore.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BasketballLiveScore.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public UserRegistrationDto RegisterInput { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult>
    OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("BasketballAPI");
                var json = JsonSerializer.Serialize(RegisterInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Authentication/Register", content);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Compte créé avec succès ! Vous pouvez maintenant vous connecter.";
                    ModelState.Clear();
                    RegisterInput = new UserRegistrationDto();

                    // Redirection après 2 secondes
                    Response.Headers.Add("Refresh", "2; url=/Login");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Erreur lors de l'inscription: {error}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de connexion: {ex.Message}";
            }

            return Page();
        }
    }
}