using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs.Player;
using BasketballLiveScore.Models;
using BasketballLiveScore.Models.Enums;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service pour la gestion des joueurs
    /// </summary>
    public class PlayerService : IPlayerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PlayerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Récupère tous les joueurs
        /// </summary>
        public async Task<IEnumerable<PlayerDto>> GetAllPlayersAsync()
        {
            var players = await _unitOfWork.Players.GetAllAsync();

            return players.Select(ConvertToDto);
        }

        /// <summary>
        /// Récupère un joueur par son identifiant
        /// </summary>
        public async Task<PlayerDto> GetPlayerByIdAsync(int id)
        {
            var player = await _unitOfWork.Players.GetByIdAsync(id);

            if (player == null)
                return null;

            return ConvertToDto(player);
        }

        /// <summary>
        /// Récupère les joueurs d'une équipe
        /// </summary>
        public async Task<IEnumerable<PlayerDto>> GetPlayersByTeamAsync(int teamId)
        {
            var players = await _unitOfWork.Players.GetPlayersByTeamAsync(teamId);

            return players.Select(ConvertToDto);
        }

        /// <summary>
        /// Crée un nouveau joueur
        /// </summary>
        public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto)
        {
            if (createPlayerDto == null)
                throw new ArgumentNullException(nameof(createPlayerDto));

            // Vérifier que l'équipe existe
            var team = await _unitOfWork.Teams.GetByIdAsync(createPlayerDto.TeamId);
            if (team == null)
                throw new InvalidOperationException($"L'équipe avec l'ID {createPlayerDto.TeamId} n'existe pas");

            // Vérifier que le numéro n'est pas déjà pris dans l'équipe
            if (await _unitOfWork.Players.IsNumberTakenInTeamAsync(createPlayerDto.JerseyNumber, createPlayerDto.TeamId))
                throw new InvalidOperationException($"Le numéro {createPlayerDto.JerseyNumber} est déjà utilisé dans cette équipe");

            var player = new Player
            {
                FirstName = createPlayerDto.FirstName,
                LastName = createPlayerDto.LastName,
                JerseyNumber = createPlayerDto.JerseyNumber,
                TeamId = createPlayerDto.TeamId
            };

            await _unitOfWork.Players.AddAsync(player);
            await _unitOfWork.CompleteAsync();

            return ConvertToDto(player);
        }

        /// <summary>
        /// Met à jour un joueur existant
        /// </summary>
        public async Task<PlayerDto> UpdatePlayerAsync(int id, UpdatePlayerDto updatePlayerDto)
        {
            if (updatePlayerDto == null)
                throw new ArgumentNullException(nameof(updatePlayerDto));

            var player = await _unitOfWork.Players.GetByIdAsync(id);

            if (player == null)
                return null;

            // Mise à jour des propriétés si elles sont fournies
            if (!string.IsNullOrEmpty(updatePlayerDto.FirstName))
                player.FirstName = updatePlayerDto.FirstName;

            if (!string.IsNullOrEmpty(updatePlayerDto.LastName))
                player.LastName = updatePlayerDto.LastName;

            if (updatePlayerDto.JerseyNumber.HasValue)
            {
                // Vérifier que le nouveau numéro n'est pas déjà pris
                if (player.JerseyNumber != updatePlayerDto.JerseyNumber.Value)
                {
                    if (await _unitOfWork.Players.IsNumberTakenInTeamAsync(updatePlayerDto.JerseyNumber.Value, player.TeamId))
                        throw new InvalidOperationException($"Le numéro {updatePlayerDto.JerseyNumber.Value} est déjà utilisé dans cette équipe");

                    player.JerseyNumber = updatePlayerDto.JerseyNumber.Value;
                }
            }

            if (updatePlayerDto.TeamId.HasValue && updatePlayerDto.TeamId.Value != player.TeamId)
            {
                // Vérifier que la nouvelle équipe existe
                var team = await _unitOfWork.Teams.GetByIdAsync(updatePlayerDto.TeamId.Value);
                if (team == null)
                    throw new InvalidOperationException($"L'équipe avec l'ID {updatePlayerDto.TeamId.Value} n'existe pas");

                // Vérifier que le numéro n'est pas déjà pris dans la nouvelle équipe
                if (await _unitOfWork.Players.IsNumberTakenInTeamAsync(player.JerseyNumber, updatePlayerDto.TeamId.Value))
                    throw new InvalidOperationException($"Le numéro {player.JerseyNumber} est déjà utilisé dans l'équipe cible");

                player.TeamId = updatePlayerDto.TeamId.Value;
            }

            _unitOfWork.Players.Update(player);
            await _unitOfWork.CompleteAsync();

            return ConvertToDto(player);
        }

        /// <summary>
        /// Supprime un joueur
        /// </summary>
        public async Task<bool> DeletePlayerAsync(int id)
        {
            var player = await _unitOfWork.Players.GetByIdAsync(id);

            if (player == null)
                return false;

            _unitOfWork.Players.Remove(player);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Récupère les statistiques d'un joueur pour un match
        /// </summary>
        public async Task<PlayerMatchStatsDto> GetPlayerMatchStatsAsync(int playerId, int matchId)
        {
            var player = await _unitOfWork.Players.GetByIdAsync(playerId);
            if (player == null)
                return null;

            var matchEvents = await _unitOfWork.MatchEvents. GetMatchEventsAsync(matchId);
            var playerEvents = matchEvents.Where(e => e.PlayerId == playerId);

            return new PlayerMatchStatsDto
            {
                PlayerId = player.Id,
                PlayerName = player.FullName,
                JerseyNumber = player.JerseyNumber,
                Points = playerEvents.Where(e => e.EventType == MatchEventType.Basket).Sum(e => e.Points),
                Fouls = playerEvents.Count(e => e.EventType == MatchEventType.Foul),
                MinutesPlayed = 0, // À calculer selon la logique métier
                IsStarter = false // À déterminer selon le lineup du match
            };
        }

        /// <summary>
        /// Méthode privée pour convertir un Player en PlayerDto
        /// </summary>
        private PlayerDto ConvertToDto(Player player)
        {
            return new PlayerDto
            {
                Id = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                FullName = player.FullName,
                JerseyNumber = player.JerseyNumber,
                TeamId = player.TeamId,
                TeamName = player.Team?.Name ?? string.Empty
            };
        }
    }
}