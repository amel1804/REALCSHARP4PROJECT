using BasketballLiveScore.DTOs.Player;
using BasketballLiveScore.DTOs.Team;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BasketballLiveScore.Pages
{
	public class PlayersModel : PageModel
	{
		private const string HTTP_CLIENT_NAME = "BasketballAPI";
		private const string TOKEN_SESSION_KEY = "Token";

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<PlayersModel> _logger;

		public PlayersModel(IHttpClientFactory httpClientFactory, ILogger<PlayersModel> logger)
		{
			_httpClientFactory = httpClientFactory;
			_logger = logger;
		}

		public List<PlayerDto> Players { get; set; } = new();
		public List<TeamDto> Teams { get; set; } = new();

		[BindProperty(SupportsGet = true)]
		public int? TeamId { get; set; }

		[BindProperty]
		public CreatePlayerDto NewPlayer { get; set; } = new();

		[BindProperty]
		public UpdatePlayerInputModel UpdatePlayer { get; set; } = new();

		[TempData]
		public string SuccessMessage { get; set; } = string.Empty;

		[TempData]
		public string ErrorMessage { get; set; } = string.Empty;

		public class UpdatePlayerInputModel
		{
			public int Id { get; set; }
			public string FirstName { get; set; } = string.Empty;
			public string LastName { get; set; } = string.Empty;
			public int JerseyNumber { get; set; }
			public int TeamId { get; set; }
		}

		public async Task<IActionResult> OnGetAsync()
		{
			var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
			if (string.IsNullOrEmpty(token))
			{
				return RedirectToPage("/Login");
			}

			await LoadDataAsync(token);
			return Page();
		}

		public async Task<IActionResult> OnPostCreateAsync()
		{
			var token = HttpContext.Session.GetString(TOKEN_SESSION_KEY);
			if (string.IsNullOrEmpty(token))
			{
				return RedirectToPage("/Login");
			}

			try
			{
				var client = CreateAuthenticatedClient(token);
				var json = JsonSerializer.Serialize(NewPlayer);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await client.PostAsync("api/Player", content);

				if (response.IsSuccessStatusCode)
				{
					SuccessMessage = "Joueur créé avec succès";
				}
				else
				{
					var error = await response.Content.ReadAsStringAsync();
					ErrorMessage = $"Erreur: {error}";
				}
			}
			catch (Exception ex)
			{
				ErrorMessage = $"Erreur: {ex.Message}";
				_logger.LogError(ex, "Erreur lors de la création du joueur");
			}

			return RedirectToPage();
		}

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

				var updateDto = new UpdatePlayerDto
				{
					FirstName = UpdatePlayer.FirstName,
					LastName = UpdatePlayer.LastName,
					JerseyNumber = UpdatePlayer.JerseyNumber,
					TeamId = UpdatePlayer.TeamId
				};

				var json = JsonSerializer.Serialize(updateDto);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await client.PutAsync($"api/Player/{UpdatePlayer.Id}", content);

				if (response.IsSuccessStatusCode)
				{
					SuccessMessage = "Joueur modifié avec succès";
				}
				else
				{
					var error = await response.Content.ReadAsStringAsync();
					ErrorMessage = $"Erreur: {error}";
				}
			}
			catch (Exception ex)
			{
				ErrorMessage = $"Erreur: {ex.Message}";
				_logger.LogError(ex, "Erreur lors de la modification du joueur");
			}

			return RedirectToPage();
		}

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
				var response = await client.DeleteAsync($"api/Player/{id}");

				if (response.IsSuccessStatusCode)
				{
					SuccessMessage = "Joueur supprimé avec succès";
				}
				else
				{
					ErrorMessage = "Erreur lors de la suppression";
				}
			}
			catch (Exception ex)
			{
				ErrorMessage = $"Erreur: {ex.Message}";
				_logger.LogError(ex, "Erreur lors de la suppression du joueur");
			}

			return RedirectToPage();
		}

		private async Task LoadDataAsync(string token)
		{
			try
			{
				var client = CreateAuthenticatedClient(token);

				// Charger les équipes
				var teamsResponse = await client.GetAsync("api/Team");
				if (teamsResponse.IsSuccessStatusCode)
				{
					var teamsJson = await teamsResponse.Content.ReadAsStringAsync();
					Teams = JsonSerializer.Deserialize<List<TeamDto>>(teamsJson,
						new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
				}

				// Charger les joueurs
				string endpoint = TeamId.HasValue
					? $"api/Player/team/{TeamId.Value}"
					: "api/Player";

				var playersResponse = await client.GetAsync(endpoint);
				if (playersResponse.IsSuccessStatusCode)
				{
					var playersJson = await playersResponse.Content.ReadAsStringAsync();
					Players = JsonSerializer.Deserialize<List<PlayerDto>>(playersJson,
						new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erreur lors du chargement des données");
			}
		}

		private HttpClient CreateAuthenticatedClient(string token)
		{
			var client = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			return client;
		}
	}
}