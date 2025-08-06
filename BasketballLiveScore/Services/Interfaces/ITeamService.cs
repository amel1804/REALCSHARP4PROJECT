using System.Collections.Generic;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs.Team;
using BasketballLiveScore.DTOs.Player;

namespace BasketballLiveScore.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service de gestion des équipes
    /// </summary>
    public interface ITeamService
    {
        /// <summary>
        /// Récupère toutes les équipes
        /// </summary>
        Task<IEnumerable<TeamDto>> GetAllTeamsAsync();

        /// <summary>
        /// Récupère une équipe par son identifiant
        /// </summary>
        Task<TeamDetailDto> GetTeamByIdAsync(int id);

        /// <summary>
        /// Crée une nouvelle équipe
        /// </summary>
        Task<TeamDto> CreateTeamAsync(CreateTeamDto createTeamDto);

        /// <summary>
        /// Met à jour une équipe existante
        /// </summary>
        Task<TeamDto> UpdateTeamAsync(int id, UpdateTeamDto updateTeamDto);

        /// <summary>
        /// Supprime une équipe
        /// </summary>
        Task<bool> DeleteTeamAsync(int id);

        /// <summary>
        /// Récupère les joueurs d'une équipe
        /// </summary>
        Task<IEnumerable<PlayerSummaryDto>> GetTeamPlayersAsync(int teamId);
    }
}