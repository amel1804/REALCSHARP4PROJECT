using BasketballLiveScore.DTOs.LiveScore;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace BasketballLiveScore.Hubs
{
    /// <summary>
    /// Hub SignalR pour la diffusion en temps réel des scores
    /// </summary>
    [Authorize]
    public class LiveScoreHub : Hub
    {
        private const string MATCH_GROUP_PREFIX = "match-";
        private const string ADMIN_GROUP = "admins";
        private const string ENCODER_GROUP = "encoders";

        private const string CLIENT_UPDATE_SCORE = "UpdateScore";
        private const string CLIENT_UPDATE_CLOCK = "UpdateClock";
        private const string CLIENT_NEW_EVENT = "NewEvent";
        private const string CLIENT_PLAYER_SUBSTITUTION = "PlayerSubstitution";
        private const string CLIENT_QUARTER_CHANGE = "QuarterChange";
        private const string CLIENT_MATCH_STATUS = "MatchStatusChange";
        private const string CLIENT_TIMEOUT = "TimeoutCalled";

        private readonly ILiveScoreService _liveScoreService;
        private readonly IMatchService _matchService;

        public LiveScoreHub(ILiveScoreService liveScoreService, IMatchService matchService)
        {
            _liveScoreService = liveScoreService ?? throw new ArgumentNullException(nameof(liveScoreService));
            _matchService = matchService ?? throw new ArgumentNullException(nameof(matchService));
        }

        public override async Task OnConnectedAsync()
        {
            var userRole = Context.User?.FindFirst("role")?.Value;

            if (userRole == "Administrator")
                await Groups.AddToGroupAsync(Context.ConnectionId, ADMIN_GROUP);
            else if (userRole == "Encoder")
                await Groups.AddToGroupAsync(Context.ConnectionId, ENCODER_GROUP);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinMatch(int matchId)
        {
            var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var liveScore = await _liveScoreService.GetLiveScoreAsync(matchId);
            await Clients.Caller.SendAsync(CLIENT_UPDATE_SCORE, liveScore);
        }

        public async Task LeaveMatch(int matchId)
        {
            var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        [Authorize(Roles = "Administrator,Encoder")]
        public async Task RecordBasket(int matchId, BasketScoreDto basketDto)
        {
            try
            {
                var success = await _liveScoreService.RecordBasketAsync(matchId, basketDto);

                if (success)
                {
                    var liveScore = await _liveScoreService.GetLiveScoreAsync(matchId);
                    var groupName = $"{MATCH_GROUP_PREFIX}{matchId}";

                    await Clients.Group(groupName).SendAsync(CLIENT_UPDATE_SCORE, liveScore);

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
