using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs.Player;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Interface pour le service de gestion des joueurs
    /// </summary>
    public interface IPlayerService
    {
        Task<Player> GetByIdAsync(int id);
        Task<IEnumerable<Player>> GetAllAsync();
        Task<IEnumerable<Player>> GetByTeamAsync(int teamId);
        Task<Player> CreateAsync(PlayerDto playerDto);
        Task<Player> UpdateAsync(int id, PlayerDto playerDto);
        Task<bool> DeleteAsync(int id);
        Task<PlayerMatchStatsDto> GetPlayerMatchStatsAsync(int playerId, int matchId);
        Task<bool> UpdateJerseyNumberAsync(int playerId, int newNumber);
    }

    /// <summary>
    /// Service pour la gestion des joueurs de basketball
    /// </summary>
    public class PlayerService : IPlayerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PlayerService> _logger;

        public PlayerService(IUnitOfWork unitOfWork, ILogger<PlayerService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Player> GetByIdAsync(int id)
        {
            try
            {
                var player = _unitOfWork.Players.GetPlayerWithStats(id);
                return await Task.FromResult(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration du joueur {PlayerId}", id);
                return null;
            }
        }

        public async Task<IEnumerable<Player>> GetAllAsync()
        {
            try
            {
                var players = _unitOfWork.Players.GetAll();
                return await Task.FromResult(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration des joueurs");
                return new List<Player>();
            }
        }

        public async Task<IEnumerable<Player>> GetByTeamAsync(int teamId)
        {
            try
            {
                var players = _unitOfWork.Players.GetPlayersByTeam(teamId);
                return await Task.FromResult(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration des joueurs de l'�quipe {TeamId}", teamId);
                return new List<Player>();
            }
        }

        public async Task<Player> CreateAsync(PlayerDto playerDto)
        {
            if (playerDto == null)
                throw new ArgumentNullException(nameof(playerDto));

            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(playerDto.FirstName) || string.IsNullOrWhiteSpace(playerDto.LastName))
                {
                    throw new ArgumentException("Le pr�nom et le nom sont obligatoires");
                }

                if (playerDto.JerseyNumber < 0 || playerDto.JerseyNumber > 99)
                {
                    throw new ArgumentException("Le num�ro de maillot doit �tre entre 0 et 99");
                }

                // V�rifier l'�quipe existe
                var team = _unitOfWork.Teams.GetById(playerDto.TeamId);
                if (team == null)
                {
                    throw new InvalidOperationException($"L'�quipe avec l'ID {playerDto.TeamId} n'existe pas");
                }

                // V�rifier l'unicit� du num�ro dans l'�quipe
                var existingNumber = _unitOfWork.Players
                    .Find(p => p.TeamId == playerDto.TeamId && p.JerseyNumber == playerDto.JerseyNumber)
                    .FirstOrDefault();

                if (existingNumber != null)
                {
                    throw new InvalidOperationException($"Le num�ro {playerDto.JerseyNumber} est d�j� utilis� dans cette �quipe");
                }

                var player = new Player
                {
                    FirstName = playerDto.FirstName,
                    LastName = playerDto.LastName,
                    JerseyNumber = playerDto.JerseyNumber,
                    TeamId = playerDto.TeamId
                };

                _unitOfWork.Players.Add(player);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Joueur {PlayerName} cr�� avec succ�s", player.FullName);
                return player;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la cr�ation du joueur");
                throw;
            }
        }

        public async Task<Player> UpdateAsync(int id, PlayerDto playerDto)
        {
            if (playerDto == null)
                throw new ArgumentNullException(nameof(playerDto));

            try
            {
                var player = _unitOfWork.Players.GetById(id);
                if (player == null)
                {
                    _logger.LogWarning("Joueur {PlayerId} non trouv�", id);
                    return null;
                }

                // Validation du num�ro si chang�
                if (player.JerseyNumber != playerDto.JerseyNumber)
                {
                    var existingNumber = _unitOfWork.Players
                        .Find(p => p.TeamId == player.TeamId &&
                                   p.JerseyNumber == playerDto.JerseyNumber &&
                                   p.Id != id)
                        .FirstOrDefault();

                    if (existingNumber != null)
                    {
                        throw new InvalidOperationException($"Le num�ro {playerDto.JerseyNumber} est d�j� utilis� dans cette �quipe");
                    }
                }

                // Mise � jour des propri�t�s
                player.FirstName = playerDto.FirstName;
                player.LastName = playerDto.LastName;
                player.JerseyNumber = playerDto.JerseyNumber;

                if (playerDto.TeamId != player.TeamId)
                {
                    // Changement d'�quipe
                    var newTeam = _unitOfWork.Teams.GetById(playerDto.TeamId);
                    if (newTeam == null)
                    {
                        throw new InvalidOperationException($"L'�quipe avec l'ID {playerDto.TeamId} n'existe pas");
                    }
                    player.TeamId = playerDto.TeamId;
                }

                _unitOfWork.Players.Update(player);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Joueur {PlayerId} mis � jour avec succ�s", id);
                return player;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise � jour du joueur {PlayerId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var player = _unitOfWork.Players.GetById(id);
                if (player == null)
                {
                    _logger.LogWarning("Joueur {PlayerId} non trouv� pour suppression", id);
                    return false;
                }

                // V�rifier qu'il n'y a pas de statistiques de match associ�es
                var hasMatchStats = _unitOfWork.MatchLineups
                    .Find(ml => ml.PlayerId == id)
                    .Any();

                if (hasMatchStats)
                {
                    _logger.LogWarning("Impossible de supprimer le joueur {PlayerId} car il a des statistiques de match", id);
                    return false;
                }

                _unitOfWork.Players.Remove(player);
                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Joueur {PlayerId} supprim� avec succ�s", id);
                }

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du joueur {PlayerId}", id);
                return false;
            }
        }

        public async Task<PlayerMatchStatsDto> GetPlayerMatchStatsAsync(int playerId, int matchId)
        {
            try
            {
                var lineup = _unitOfWork.MatchLineups.GetPlayerLineup(matchId, playerId);
                if (lineup == null)
                {
                    _logger.LogWarning("Statistiques non trouv�es pour le joueur {PlayerId} dans le match {MatchId}",
                        playerId, matchId);
                    return null;
                }

                var stats = new PlayerMatchStatsDto
                {
                    PlayerId = playerId,
                    PlayerName = lineup.Player?.FullName ?? "Unknown",
                    JerseyNumber = lineup.Player?.JerseyNumber ?? 0,
                    Points = lineup.Points,
                    Fouls = lineup.PersonalFouls,
                    MinutesPlayed = lineup.PlayingTimeSeconds / 60,
                    IsStarter = lineup.IsStarter
                };

                return await Task.FromResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration des statistiques du joueur");
                return null;
            }
        }

        public async Task<bool> UpdateJerseyNumberAsync(int playerId, int newNumber)
        {
            try
            {
                if (newNumber < 0 || newNumber > 99)
                {
                    _logger.LogWarning("Num�ro de maillot invalide: {Number}", newNumber);
                    return false;
                }

                var player = _unitOfWork.Players.GetById(playerId);
                if (player == null)
                {
                    _logger.LogWarning("Joueur {PlayerId} non trouv�", playerId);
                    return false;
                }

                // V�rifier l'unicit� du num�ro dans l'�quipe
                var existingNumber = _unitOfWork.Players
                    .Find(p => p.TeamId == player.TeamId &&
                               p.JerseyNumber == newNumber &&
                               p.Id != playerId)
                    .FirstOrDefault();

                if (existingNumber != null)
                {
                    _logger.LogWarning("Le num�ro {Number} est d�j� utilis� dans l'�quipe", newNumber);
                    return false;
                }

                player.JerseyNumber = newNumber;
                _unitOfWork.Players.Update(player);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Num�ro de maillot du joueur {PlayerId} chang� en {Number}",
                    playerId, newNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de num�ro de maillot");
                return false;
            }
        }
    }
}