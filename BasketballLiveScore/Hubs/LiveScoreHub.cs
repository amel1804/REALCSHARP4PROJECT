
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using BasketballLiveScore.DTOs.LiveScore;
using BasketballLiveScore.Services.Interfaces;

namespace BasketballLiveScore.Hubs
{
    /// <summary>
    /// Hub SignalR pour la diffusion en temps réel des scores
    /// Permet la communication bidirectionnelle avec les clients
    /// </summary>
    [Authorize]
    public class LiveScoreHub : Hub
    {
        // Constantes pour les noms de groupes et méthodes
        private const string MATCH_GROUP_PREFIX = "match-";
        private const string ADMIN_GROUP = "admins";
        private const string ENCODER_GROUP = "encoders";

        // Noms des méthodes côté client
        private const string CLIENT_UPDATE_SCORE = "UpdateScore";
        private const string CLIENT_UPDATE_CLOCK = "UpdateClock";
        private const string CLIENT_NEW_EVENT = "NewEvent";
        private const string CLIENT_PLAYER_SUBSTITUTION = "PlayerSubstitution";
        private const string CLIENT_QUARTER_CHANGE = "QuarterChange";
        private const string CLIENT_MATCH_STATUS = "MatchStatusChange";
        private const string CLIENT_TIMEOUT = "TimeoutCalled";

        private readonly ILiveScoreService _liveScoreService;
        private readonly IMatchService _matchService;

        /// <summary>
        /// Constructeur avec injection de dépendances
        /// </summary>
        public LiveScoreHub(ILiveScoreService liveScoreService, IMatchService matchService)
        {
            _liveScoreService = liveScoreService ?? throw new ArgumentNullException(nameof(liveScoreService));
            _matchService = matchService ?? throw new ArgumentNullException(nameof(matchService));
        }

        /// <summary>
        /// Méthode appelée lors de la connexion d'un client
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            // Ajouter l'utilisateur aux groupes appropriés selon son rôle
            var userRole = Context.User?.FindFirst("role")?.Value;

            if (userRole == "Administrator")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ADMIN_GROUP);
            }
            else if (userRole == "Encoder")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ENCODER_GROUP);
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Méthode appelée lors de la déconnexion d'un client
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Retirer de tous les groupes automatiquement géré par SignalR
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Rejoint un groupe de match pour recevoir les mises à jour
        /// </summary>
        public async Task JoinMatch(int matchId)
        {
            var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Envoyer l'état actuel du match au nouveau client
            var liveScore = await _liveScoreService.GetLiveScoreAsync(matchId);
            await Clients.Caller.SendAsync(CLIENT_UPDATE_SCORE, liveScore);
        }

        /// <summary>
        /// Quitte un groupe de match
        /// </summary>
        public async Task LeaveMatch(int matchId)
        {
            var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Enregistre un panier et diffuse la mise à jour
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task RecordBasket(int matchId, BasketScoreDto basketDto)
        {
            try
            {
                // Enregistrer le panier
                var success = await _liveScoreService.RecordBasketAsync(matchId, basketDto);

                if (success)
                {
                    // Récupérer le score mis à jour
                    var liveScore = await _liveScoreService.GetLiveScoreAsync(matchId);

                    // Diffuser à tous les clients du match
                    var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";
                    await Clients.Group(groupName).SendAsync(CLIENT_UPDATE_SCORE, liveScore);

                    // Créer l'événement
                    var eventDto = new RecentEventDto
                    {
                        EventTime = DateTime.UtcNow,
                        Quarter = basketDto.Quarter,
                        GameClock = basketDto.GameTime.ToString(@"mm\:ss"),
                        EventType = "Basket",
                        Description = $"{basketDto.Points} point(s) marqué(s)"
                    };

                    await Clients.Group(groupName).SendAsync(CLIENT_NEW_EVENT, eventDto);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Erreur lors de l'enregistrement du panier: {ex.Message}");
            }
        }

        /// <summary>
        /// Enregistre une faute et diffuse la mise à jour
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task RecordFoul(int matchId, FoulCommittedDto foulDto)
        {
            try
            {
                var success = await _liveScoreService.RecordFoulAsync(matchId, foulDto);

                if (success)
                {
                    var liveScore = await _liveScoreService.GetLiveScoreAsync(matchId);
                    var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";

                    await Clients.Group(groupName).SendAsync(CLIENT_UPDATE_SCORE, liveScore);

                    var eventDto = new RecentEventDto
                    {
                        EventTime = DateTime.UtcNow,
                        Quarter = foulDto.Quarter,
                        GameClock = foulDto.GameTime.ToString(@"mm\:ss"),
                        EventType = "Foul",
                        Description = $"Faute {foulDto.FoulType}"
                    };

                    await Clients.Group(groupName).SendAsync(CLIENT_NEW_EVENT, eventDto);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Erreur lors de l'enregistrement de la faute: {ex.Message}");
            }
        }

        /// <summary>
        /// Effectue une substitution et diffuse la mise à jour
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task RecordSubstitution(int matchId, PlayerSubstitutionDto substitutionDto)
        {
            try
            {
                var success = await _liveScoreService.RecordSubstitutionAsync(matchId, substitutionDto);

                if (success)
                {
                    var liveScore = await _liveScoreService.GetLiveScoreAsync(matchId);
                    var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";

                    await Clients.Group(groupName).SendAsync(CLIENT_PLAYER_SUBSTITUTION, liveScore.TeamsOnCourt);

                    var eventDto = new RecentEventDto
                    {
                        EventTime = DateTime.UtcNow,
                        Quarter = substitutionDto.Quarter,
                        GameClock = substitutionDto.GameTime.ToString(@"mm\:ss"),
                        EventType = "Substitution",
                        Description = "Changement de joueur"
                    };

                    await Clients.Group(groupName).SendAsync(CLIENT_NEW_EVENT, eventDto);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Erreur lors de la substitution: {ex.Message}");
            }
        }

        /// <summary>
        /// Appelle un temps mort et diffuse la mise à jour
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task CallTimeout(int matchId, int teamId)
        {
            try
            {
                var success = await _liveScoreService.RecordTimeoutAsync(matchId, teamId);

                if (success)
                {
                    var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";
                    await Clients.Group(groupName).SendAsync(CLIENT_TIMEOUT, teamId);

                    var eventDto = new RecentEventDto
                    {
                        EventTime = DateTime.UtcNow,
                        EventType = "Timeout",
                        Description = "Temps mort demandé"
                    };

                    await Clients.Group(groupName).SendAsync(CLIENT_NEW_EVENT, eventDto);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Erreur lors du temps mort: {ex.Message}");
            }
        }

        /// <summary>
        /// Met à jour le chronomètre du match
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task UpdateGameClock(int matchId, UpdateGameClockDto clockDto)
        {
            try
            {
                var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";

                var gameClock = new GameClockDto
                {
                    RemainingSeconds = clockDto.GameClockSeconds,
                    IsRunning = clockDto.IsRunning,
                    CurrentQuarter = 1 // À récupérer du match
                };

                await Clients.Group(groupName).SendAsync(CLIENT_UPDATE_CLOCK, gameClock);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Erreur lors de la mise à jour du chrono: {ex.Message}");
            }
        }

        /// <summary>
        /// Change de quart-temps
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task ChangeQuarter(int matchId, int newQuarter)
        {
            try
            {
                var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";
                await Clients.Group(groupName).SendAsync(CLIENT_QUARTER_CHANGE, newQuarter);

                var eventDto = new RecentEventDto
                {
                    EventTime = DateTime.UtcNow,
                    Quarter = newQuarter,
                    EventType = "QuarterChange",
                    Description = $"Début du quart-temps {newQuarter}"
                };

                await Clients.Group(groupName).SendAsync(CLIENT_NEW_EVENT, eventDto);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Erreur lors du changement de quart-temps: {ex.Message}");
            }
        }

        /// <summary>
        /// Démarre le chronomètre
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task StartClock(int matchId)
        {
            try
            {
                var success = await _liveScoreService.StartClockAsync(matchId);

                if (success)
                {
                    var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";
                    await Clients.Group(groupName).SendAsync(CLIENT_MATCH_STATUS, "InProgress");
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Erreur lors du démarrage du chrono: {ex.Message}");
            }
        }

        /// <summary>
        /// Arrête le chronomètre
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task StopClock(int matchId)
        {
            try
            {
                var success = await _liveScoreService.StopClockAsync(matchId);

                if (success)
                {
                    var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";
                    await Clients.Group(groupName).SendAsync(CLIENT_MATCH_STATUS, "Paused");
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Erreur lors de l'arrêt du chrono: {ex.Message}");
            }
        }
    }
}