using System.Collections.Generic;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs.Player;

namespace BasketballLiveScore.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service de gestion des joueurs
    /// </summary>
    public interface IPlayerService
    {
        /// <summary>
        /// R�cup�re tous les joueurs
        /// </summary>
        Task<IEnumerable<PlayerDto>> GetAllPlayersAsync();

        /// <summary>
        /// R�cup�re un joueur par son identifiant
        /// </summary>
        Task<PlayerDto?> GetPlayerByIdAsync(int id);

        /// <summary>
        /// R�cup�re les joueurs d'une �quipe
        /// </summary>
        Task<IEnumerable<PlayerDto>> GetPlayersByTeamAsync(int teamId);

        /// <summary>
        /// Cr�e un nouveau joueur
        /// </summary>
        Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto);

        /// <summary>
        /// Met � jour un joueur existant
        /// </summary>
        Task<PlayerDto> UpdatePlayerAsync(int id, UpdatePlayerDto updatePlayerDto);
        /// <summary>
        /// D�finit les 5 joueurs de base pour un match
        /// </summary>
        Task SetStartingFiveAsync(int matchId, int teamId, List<int> playerIds);

        /// <summary>
        /// Supprime un joueur
        /// </summary>
        Task<bool> DeletePlayerAsync(int id);

        /// <summary>
        /// R�cup�re les statistiques d'un joueur pour un match
        /// </summary>
        Task<PlayerMatchStatsDto> GetPlayerMatchStatsAsync(int playerId, int matchId);
    }
}