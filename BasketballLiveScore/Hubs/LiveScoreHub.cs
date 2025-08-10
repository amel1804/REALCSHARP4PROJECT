using BasketballLiveScore.DTOs.LiveScore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

namespace BasketballLiveScore.Hubs
{
    /// <summary>
    /// Hub SignalR pour la diffusion en temps r�el des scores et �v�nements de match
    /// </summary>
    [Authorize]
    public class LiveScoreHub : Hub
    {
        private readonly ILogger<LiveScoreHub> _logger;

        public LiveScoreHub(ILogger<LiveScoreHub> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Appel� quand un client se connecte au hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            _logger.LogInformation("Client connect�: {ConnectionId} - User: {UserId}", connectionId, userId);

            await Clients.Caller.SendAsync("Connected", connectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Appel� quand un client se d�connecte du hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            if (exception != null)
            {
                _logger.LogError(exception, "Client d�connect� avec erreur: {ConnectionId}", connectionId);
            }
            else
            {
                _logger.LogInformation("Client d�connect�: {ConnectionId} - User: {UserId}", connectionId, userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Rejoint un groupe de match pour recevoir les mises � jour
        /// </summary>
        public async Task JoinMatch(int matchId)
        {
            var groupName = GetMatchGroupName(matchId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("Client {ConnectionId} a rejoint le match {MatchId}",
                Context.ConnectionId, matchId);

            await Clients.Caller.SendAsync("JoinedMatch", matchId);
        }

        /// <summary>
        /// Quitte un groupe de match
        /// </summary>
        public async Task LeaveMatch(int matchId)
        {
            var groupName = GetMatchGroupName(matchId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("Client {ConnectionId} a quitt� le match {MatchId}",
                Context.ConnectionId, matchId);

            await Clients.Caller.SendAsync("LeftMatch", matchId);
        }

        /// <summary>
        /// Diffuse la mise � jour du score � tous les clients du match
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task UpdateScore(int matchId, int homeScore, int awayScore)
        {
            var groupName = GetMatchGroupName(matchId);

            await Clients.Group(groupName).SendAsync("ScoreUpdated", new
            {
                matchId,
                homeScore,
                awayScore,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Score mis � jour pour le match {MatchId}: {HomeScore}-{AwayScore}",
                matchId, homeScore, awayScore);
        }

        /// <summary>
        /// Diffuse un panier marqu�
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task BroadcastBasket(int matchId, BasketScoredDto basketDto)
        {
            var groupName = GetMatchGroupName(matchId);

            await Clients.Group(groupName).SendAsync("BasketScored", new
            {
                matchId,
                playerId = basketDto.PlayerId,
                points = basketDto.Points,
                quarter = basketDto.Quarter,
                gameTime = basketDto.GameTime,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Panier de {Points} points diffus� pour le match {MatchId}",
                basketDto.Points, matchId);
        }

        /// <summary>
        /// Diffuse une faute commise
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task BroadcastFoul(int matchId, FoulCommittedDto foulDto)
        {
            var groupName = GetMatchGroupName(matchId);

            await Clients.Group(groupName).SendAsync("FoulCommitted", new
            {
                matchId,
                playerId = foulDto.PlayerId,
                foulType = foulDto.FoulType,
                quarter = foulDto.Quarter,
                gameTime = foulDto.GameTime,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Faute {FoulType} diffus�e pour le match {MatchId}",
                foulDto.FoulType, matchId);
        }

        /// <summary>
        /// Diffuse un changement de joueur
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task BroadcastSubstitution(int matchId, PlayerSubstitutionDto substitutionDto)
        {
            var groupName = GetMatchGroupName(matchId);

            await Clients.Group(groupName).SendAsync("PlayerSubstitution", new
            {
                matchId,
                playerInId = substitutionDto.PlayerInId,
                playerOutId = substitutionDto.PlayerOutId,
                quarter = substitutionDto.Quarter,
                gameTime = substitutionDto.GameTime,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Changement diffus� pour le match {MatchId}: {PlayerOut} -> {PlayerIn}",
                matchId, substitutionDto.PlayerOutId, substitutionDto.PlayerInId);
        }

        /// <summary>
        /// Diffuse un timeout
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task BroadcastTimeout(int matchId, TimeoutCalledDto timeoutDto)
        {
            var groupName = GetMatchGroupName(matchId);

            await Clients.Group(groupName).SendAsync("TimeoutCalled", new
            {
                matchId,
                teamId = timeoutDto.TeamId,
                quarter = timeoutDto.Quarter,
                gameClockSeconds = timeoutDto.GameClockSeconds,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Timeout diffus� pour le match {MatchId}", matchId);
        }

        /// <summary>
        /// Diffuse le changement de quart-temps
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task BroadcastQuarterChange(int matchId, int newQuarter)
        {
            var groupName = GetMatchGroupName(matchId);

            await Clients.Group(groupName).SendAsync("QuarterChanged", new
            {
                matchId,
                quarter = newQuarter,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Changement de quart-temps diffus� pour le match {MatchId}: Q{Quarter}",
                matchId, newQuarter);
        }

        /// <summary>
        /// Diffuse la mise � jour du chronom�tre
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task BroadcastGameClock(int matchId, int remainingSeconds)
        {
            var groupName = GetMatchGroupName(matchId);

            await Clients.Group(groupName).SendAsync("GameClockUpdated", new
            {
                matchId,
                remainingSeconds,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Diffuse le d�but d'un match
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task BroadcastMatchStart(int matchId)
        {
            var groupName = GetMatchGroupName(matchId);

            await Clients.Group(groupName).SendAsync("MatchStarted", new
            {
                matchId,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("D�but du match {MatchId} diffus�", matchId);
        }

        /// <summary>
        /// Diffuse la fin d'un match
        /// </summary>
        [Authorize(Roles = "Administrator,Encoder")]
        public async Task BroadcastMatchEnd(int matchId, int finalHomeScore, int finalAwayScore)
        {
            var groupName = GetMatchGroupName(matchId);

            await Clients.Group(groupName).SendAsync("MatchEnded", new
            {
                matchId,
                finalHomeScore,
                finalAwayScore,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Fin du match {MatchId} diffus�e. Score final: {HomeScore}-{AwayScore}",
                matchId, finalHomeScore, finalAwayScore);
        }

        /// <summary>
        /// Obtient le nom du groupe pour un match
        /// </summary>
        private string GetMatchGroupName(int matchId)
        {
            return $"match-{matchId}";
        }
    }
}