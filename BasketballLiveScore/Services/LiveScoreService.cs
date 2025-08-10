using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs.LiveScore;
using BasketballLiveScore.Models;
using BasketballLiveScore.Models.Events;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Interface pour le service LiveScore
    /// </summary>
    public interface ILiveScoreService
    {
        Task<bool> RecordBasketAsync(int matchId, BasketScoredDto basketDto, int encoderId);
        Task<bool> RecordFoulAsync(int matchId, FoulCommittedDto foulDto, int encoderId);
        Task<bool> RecordSubstitutionAsync(int matchId, PlayerSubstitutionDto substitutionDto, int encoderId);
        Task<bool> RecordTimeoutAsync(int matchId, TimeoutCalledDto timeoutDto, int encoderId);
        Task<bool> ChangeQuarterAsync(int matchId, int newQuarter);
        Task<bool> StartMatchAsync(int matchId);
        Task<bool> EndMatchAsync(int matchId);
        Task<MatchLiveStatusDto> GetMatchStatusAsync(int matchId);
        Task<bool> UpdateGameClockAsync(int matchId, int remainingSeconds);
    }

    /// <summary>
    /// Service pour gérer l'encodage en temps réel des matchs
    /// Implémente toutes les fonctionnalités de l'énoncé
    /// </summary>
    public class LiveScoreService : ILiveScoreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LiveScoreService> _logger;

        // Dictionnaire pour tracker les timeouts utilisés par équipe
        private readonly Dictionary<int, Dictionary<int, int>> _timeoutsUsed = new();

        public LiveScoreService(
            IUnitOfWork unitOfWork,
            ILogger<LiveScoreService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Enregistre un panier marqué (1, 2 ou 3 points)
        /// </summary>
        public async Task<bool> RecordBasketAsync(int matchId, BasketScoredDto basketDto, int encoderId)
        {
            try
            {
                var match = _unitOfWork.Matches.GetById(matchId);
                if (match == null || match.Status != MatchStatus.InProgress)
                {
                    _logger.LogWarning("Match {MatchId} non trouvé ou pas en cours", matchId);
                    return false;
                }

                // Vérifier que le joueur est sur le terrain
                var lineup = _unitOfWork.MatchLineups
                    .Find(ml => ml.MatchId == matchId && ml.PlayerId == basketDto.PlayerId)
                    .FirstOrDefault();

                if (lineup == null || !lineup.IsOnCourt)
                {
                    _logger.LogWarning("Joueur {PlayerId} non sur le terrain", basketDto.PlayerId);
                    return false;
                }

                // Validation des points (1, 2 ou 3)
                if (basketDto.Points < 1 || basketDto.Points > 3)
                {
                    _logger.LogWarning("Points invalides: {Points}", basketDto.Points);
                    return false;
                }

                // Mise à jour du score
                if (lineup.TeamId == match.HomeTeamId)
                {
                    match.HomeTeamScore += basketDto.Points;
                }
                else
                {
                    match.AwayTeamScore += basketDto.Points;
                }

                // Mise à jour des stats du joueur
                lineup.AddBasket(basketDto.Points);

                // Création de l'action de jeu
                var gameAction = new GameAction
                {
                    MatchId = matchId,
                    PlayerId = basketDto.PlayerId,
                    ActionType = "Basket",
                    Points = basketDto.Points,
                    Quarter = basketDto.Quarter,
                    GameTime = basketDto.GameTime,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.GameActions.Add(gameAction);
                _unitOfWork.Matches.Update(match);
                _unitOfWork.MatchLineups.Update(lineup);

                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Panier de {Points} points enregistré pour joueur {PlayerId}",
                    basketDto.Points, basketDto.PlayerId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement du panier");
                return false;
            }
        }

        /// <summary>
        /// Enregistre une faute (P0, P1, P2, P3)
        /// </summary>
        public async Task<bool> RecordFoulAsync(int matchId, FoulCommittedDto foulDto, int encoderId)
        {
            try
            {
                var match = _unitOfWork.Matches.GetById(matchId);
                if (match == null || match.Status != MatchStatus.InProgress)
                {
                    return false;
                }

                var lineup = _unitOfWork.MatchLineups
                    .Find(ml => ml.MatchId == matchId && ml.PlayerId == foulDto.PlayerId)
                    .FirstOrDefault();

                if (lineup == null)
                {
                    _logger.LogWarning("Joueur {PlayerId} non trouvé dans le lineup", foulDto.PlayerId);
                    return false;
                }

                // Validation du type de faute (P0, P1, P2, P3)
                var validFoulTypes = new[] { "P0", "P1", "P2", "P3", "T", "U", "D" };
                if (!validFoulTypes.Contains(foulDto.FoulType.ToUpper()))
                {
                    _logger.LogWarning("Type de faute invalide: {FoulType}", foulDto.FoulType);
                    return false;
                }

                // Ajouter la faute et vérifier la disqualification
                lineup.AddFoul(foulDto.FoulType.ToUpper());

                // Création de l'événement de faute
                var foulEvent = new FoulEvent
                {
                    MatchId = matchId,
                    PlayerId = foulDto.PlayerId,
                    FoulType = foulDto.FoulType.ToUpper(),
                    Quarter = foulDto.Quarter,
                    GameTime = foulDto.GameTime,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = encoderId
                };

                // Déterminer le nombre de lancers francs selon le type
                switch (foulDto.FoulType.ToUpper())
                {
                    case "P1":
                        foulEvent.FreeThrowsAwarded = 1;
                        break;
                    case "P2":
                        foulEvent.FreeThrowsAwarded = 2;
                        break;
                    case "P3":
                        foulEvent.FreeThrowsAwarded = 3;
                        break;
                }

                _unitOfWork.MatchEvents.Add(foulEvent);

                // Création de l'action de jeu
                var gameAction = new GameAction
                {
                    MatchId = matchId,
                    PlayerId = foulDto.PlayerId,
                    ActionType = "Foul",
                    FaultType = foulDto.FoulType.ToUpper(),
                    Quarter = foulDto.Quarter,
                    GameTime = foulDto.GameTime,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.GameActions.Add(gameAction);
                _unitOfWork.MatchLineups.Update(lineup);

                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Faute {FoulType} enregistrée pour joueur {PlayerId} (Total: {TotalFouls})",
                    foulDto.FoulType, foulDto.PlayerId, lineup.PersonalFouls);

                if (lineup.IsDisqualified)
                {
                    _logger.LogWarning("Joueur {PlayerId} disqualifié avec 5 fautes", foulDto.PlayerId);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de la faute");
                return false;
            }
        }

        /// <summary>
        /// Enregistre un changement de joueur
        /// </summary>
        public async Task<bool> RecordSubstitutionAsync(int matchId, PlayerSubstitutionDto substitutionDto, int encoderId)
        {
            try
            {
                var match = _unitOfWork.Matches.GetById(matchId);
                if (match == null || match.Status != MatchStatus.InProgress)
                {
                    return false;
                }

                var playerOut = _unitOfWork.MatchLineups
                    .Find(ml => ml.MatchId == matchId && ml.PlayerId == substitutionDto.PlayerOutId)
                    .FirstOrDefault();

                var playerIn = _unitOfWork.MatchLineups
                    .Find(ml => ml.MatchId == matchId && ml.PlayerId == substitutionDto.PlayerInId)
                    .FirstOrDefault();

                // Validations
                if (playerOut == null || !playerOut.IsOnCourt)
                {
                    _logger.LogWarning("Joueur sortant {PlayerId} pas sur le terrain", substitutionDto.PlayerOutId);
                    return false;
                }

                if (playerIn == null || !playerIn.CanEnterCourt())
                {
                    _logger.LogWarning("Joueur entrant {PlayerId} ne peut pas entrer", substitutionDto.PlayerInId);
                    return false;
                }

                // Vérifier même équipe
                if (playerOut.TeamId != playerIn.TeamId)
                {
                    _logger.LogWarning("Les joueurs ne sont pas de la même équipe");
                    return false;
                }

                // Effectuer le changement
                playerOut.ExitCourt();
                playerIn.EnterCourt(substitutionDto.Quarter);

                // Création de l'événement
                var substitutionEvent = new SubstitutionEvent
                {
                    MatchId = matchId,
                    PlayerInId = substitutionDto.PlayerInId,
                    PlayerOutId = substitutionDto.PlayerOutId,
                    Quarter = substitutionDto.Quarter,
                    GameTime = substitutionDto.GameTime,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = encoderId
                };

                _unitOfWork.MatchEvents.Add(substitutionEvent);

                // Création de l'action de jeu
                var gameAction = new GameAction
                {
                    MatchId = matchId,
                    ActionType = "Substitution",
                    PlayerInId = substitutionDto.PlayerInId,
                    PlayerOutId = substitutionDto.PlayerOutId,
                    Quarter = substitutionDto.Quarter,
                    GameTime = substitutionDto.GameTime,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.GameActions.Add(gameAction);
                _unitOfWork.MatchLineups.Update(playerOut);
                _unitOfWork.MatchLineups.Update(playerIn);

                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Changement: {PlayerOut} remplacé par {PlayerIn}",
                    substitutionDto.PlayerOutId, substitutionDto.PlayerInId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de joueur");
                return false;
            }
        }

        /// <summary>
        /// Enregistre un timeout
        /// </summary>
        public async Task<bool> RecordTimeoutAsync(int matchId, TimeoutCalledDto timeoutDto, int encoderId)
        {
            try
            {
                var match = _unitOfWork.Matches.GetById(matchId);
                if (match == null || match.Status != MatchStatus.InProgress)
                {
                    return false;
                }

                // Initialiser le tracking des timeouts
                if (!_timeoutsUsed.ContainsKey(matchId))
                {
                    _timeoutsUsed[matchId] = new Dictionary<int, int>();
                }

                if (!_timeoutsUsed[matchId].ContainsKey(timeoutDto.TeamId))
                {
                    _timeoutsUsed[matchId][timeoutDto.TeamId] = 0;
                }

                // Vérifier le nombre de timeouts (max 3 par équipe selon les règles standards)
                if (_timeoutsUsed[matchId][timeoutDto.TeamId] >= 3)
                {
                    _logger.LogWarning("Équipe {TeamId} n'a plus de timeouts", timeoutDto.TeamId);
                    return false;
                }

                _timeoutsUsed[matchId][timeoutDto.TeamId]++;

                // Création de l'événement
                var timeoutEvent = new TimeoutEvent
                {
                    MatchId = matchId,
                    TeamId = timeoutDto.TeamId,
                    Quarter = timeoutDto.Quarter,
                    GameClockSeconds = timeoutDto.GameClockSeconds,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = encoderId
                };

                _unitOfWork.MatchEvents.Add(timeoutEvent);

                // Création de l'action de jeu
                var gameAction = new GameAction
                {
                    MatchId = matchId,
                    ActionType = "Timeout",
                    Quarter = timeoutDto.Quarter,
                    GameTime = TimeSpan.FromSeconds(timeoutDto.GameClockSeconds),
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.GameActions.Add(gameAction);
                await _unitOfWork.CompleteAsync();

                var remaining = 3 - _timeoutsUsed[matchId][timeoutDto.TeamId];
                _logger.LogInformation("Timeout pour équipe {TeamId}. Restants: {Remaining}",
                    timeoutDto.TeamId, remaining);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement du timeout");
                return false;
            }
        }

        /// <summary>
        /// Change le quart-temps
        /// </summary>
        public async Task<bool> ChangeQuarterAsync(int matchId, int newQuarter)
        {
            try
            {
                var match = _unitOfWork.Matches.GetById(matchId);
                if (match == null)
                {
                    return false;
                }

                // Validation (support des prolongations jusqu'à 10)
                if (newQuarter < 1 || newQuarter > 10)
                {
                    _logger.LogWarning("Numéro de quart-temps invalide: {Quarter}", newQuarter);
                    return false;
                }

                // Mettre à jour le temps de jeu de tous les joueurs sur le terrain
                var playersOnCourt = _unitOfWork.MatchLineups
                    .Find(ml => ml.MatchId == matchId && ml.IsOnCourt)
                    .ToList();

                foreach (var player in playersOnCourt)
                {
                    player.UpdatePlayingTime();
                    _unitOfWork.MatchLineups.Update(player);
                }

                match.CurrentQuarter = newQuarter;

                // Réinitialiser le chrono
                int duration = newQuarter <= 4
                    ? match.QuarterDurationMinutes
                    : 5; // Prolongations de 5 minutes

                match.RemainingTimeSeconds = duration * 60;

                _unitOfWork.Matches.Update(match);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Passage au quart-temps {Quarter} pour match {MatchId}",
                    newQuarter, matchId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de quart-temps");
                return false;
            }
        }

        /// <summary>
        /// Démarre un match avec validation des 5 de base
        /// </summary>
        public async Task<bool> StartMatchAsync(int matchId)
        {
            try
            {
                var match = _unitOfWork.Matches.GetById(matchId);
                if (match == null || match.Status != MatchStatus.Scheduled)
                {
                    _logger.LogWarning("Match {MatchId} ne peut pas être démarré", matchId);
                    return false;
                }

                // Vérifier les 5 de base pour chaque équipe
                var homeStarters = _unitOfWork.MatchLineups
                    .Find(ml => ml.MatchId == matchId && ml.TeamId == match.HomeTeamId && ml.IsStarter)
                    .ToList();

                var awayStarters = _unitOfWork.MatchLineups
                    .Find(ml => ml.MatchId == matchId && ml.TeamId == match.AwayTeamId && ml.IsStarter)
                    .ToList();

                if (homeStarters.Count != 5 || awayStarters.Count != 5)
                {
                    _logger.LogWarning("Les 5 de base ne sont pas complets");
                    return false;
                }

                // Mettre les 5 de base sur le terrain
                foreach (var starter in homeStarters.Concat(awayStarters))
                {
                    starter.EnterCourt(1);
                    _unitOfWork.MatchLineups.Update(starter);
                }

                // Démarrer le match
                match.Status = MatchStatus.InProgress;
                match.StartTime = DateTime.UtcNow;
                match.CurrentQuarter = 1;
                match.RemainingTimeSeconds = match.QuarterDurationMinutes * 60;

                _unitOfWork.Matches.Update(match);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Match {MatchId} démarré", matchId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du démarrage du match");
                return false;
            }
        }

        /// <summary>
        /// Termine un match
        /// </summary>
        public async Task<bool> EndMatchAsync(int matchId)
        {
            try
            {
                var match = _unitOfWork.Matches.GetById(matchId);
                if (match == null || match.Status != MatchStatus.InProgress)
                {
                    return false;
                }

                // Mettre à jour le temps de jeu final
                var playersOnCourt = _unitOfWork.MatchLineups
                    .Find(ml => ml.MatchId == matchId && ml.IsOnCourt)
                    .ToList();

                foreach (var player in playersOnCourt)
                {
                    player.ExitCourt();
                    _unitOfWork.MatchLineups.Update(player);
                }

                match.Status = MatchStatus.Finished;
                match.EndTime = DateTime.UtcNow;

                _unitOfWork.Matches.Update(match);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Match {MatchId} terminé. Score: {HomeScore}-{AwayScore}",
                    matchId, match.HomeTeamScore, match.AwayTeamScore);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la fin du match");
                return false;
            }
        }

        /// <summary>
        /// Récupère le statut en direct d'un match
        /// </summary>
        public async Task<MatchLiveStatusDto> GetMatchStatusAsync(int matchId)
        {
            var match = _unitOfWork.Matches.GetById(matchId);
            if (match == null)
            {
                return new MatchLiveStatusDto { MatchId = matchId, IsActive = false };
            }

            var homePlayersOnCourt = _unitOfWork.MatchLineups
                .Find(ml => ml.MatchId == matchId && ml.TeamId == match.HomeTeamId && ml.IsOnCourt)
                .Select(ml => new PlayerOnCourtDto
                {
                    PlayerId = ml.PlayerId,
                    PlayerName = ml.Player?.FullName ?? "Unknown",
                    JerseyNumber = ml.Player?.JerseyNumber ?? 0,
                    PersonalFouls = ml.PersonalFouls,
                    Points = ml.Points
                })
                .ToList();

            var awayPlayersOnCourt = _unitOfWork.MatchLineups
                .Find(ml => ml.MatchId == matchId && ml.TeamId == match.AwayTeamId && ml.IsOnCourt)
                .Select(ml => new PlayerOnCourtDto
                {
                    PlayerId = ml.PlayerId,
                    PlayerName = ml.Player?.FullName ?? "Unknown",
                    JerseyNumber = ml.Player?.JerseyNumber ?? 0,
                    PersonalFouls = ml.PersonalFouls,
                    Points = ml.Points
                })
                .ToList();

            // Calculer les timeouts restants
            int homeTimeoutsRemaining = 3;
            int awayTimeoutsRemaining = 3;

            if (_timeoutsUsed.ContainsKey(matchId))
            {
                if (_timeoutsUsed[matchId].ContainsKey(match.HomeTeamId))
                    homeTimeoutsRemaining = 3 - _timeoutsUsed[matchId][match.HomeTeamId];

                if (_timeoutsUsed[matchId].ContainsKey(match.AwayTeamId))
                    awayTimeoutsRemaining = 3 - _timeoutsUsed[matchId][match.AwayTeamId];
            }

            return await Task.FromResult(new MatchLiveStatusDto
            {
                MatchId = matchId,
                IsActive = match.Status == MatchStatus.InProgress,
                Status = match.Status.ToString(),
                CurrentQuarter = match.CurrentQuarter,
                RemainingTimeSeconds = match.RemainingTimeSeconds,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamScore = match.AwayTeamScore,
                HomeTeamPlayers = homePlayersOnCourt,
                AwayTeamPlayers = awayPlayersOnCourt,
                HomeTeamTimeoutsRemaining = homeTimeoutsRemaining,
                AwayTeamTimeoutsRemaining = awayTimeoutsRemaining
            });
        }

        /// <summary>
        /// Met à jour le chronomètre du match
        /// </summary>
        public async Task<bool> UpdateGameClockAsync(int matchId, int remainingSeconds)
        {
            try
            {
                var match = _unitOfWork.Matches.GetById(matchId);
                if (match == null || match.Status != MatchStatus.InProgress)
                {
                    return false;
                }

                if (remainingSeconds < 0)
                {
                    remainingSeconds = 0;
                }

                match.RemainingTimeSeconds = remainingSeconds;
                _unitOfWork.Matches.Update(match);

                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du chronomètre");
                return false;
            }
        }
    }
}