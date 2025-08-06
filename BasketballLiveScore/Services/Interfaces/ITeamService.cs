using System.Collections.Generic;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs.Team;
using BasketballLiveScore.DTOs.Player;

namespace BasketballLiveScore.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service de gestion des �quipes
    /// </summary>
    public interface ITeamService
    {
        /// <summary>
        /// R�cup�re toutes les �quipes
        /// </summary>
        Task<IEnumerable<TeamDto>> GetAllTeamsAsync();

        /// <summary>
        /// R�cup�re une �quipe par son identifiant
        /// </summary>
        Task<TeamDetailDto> GetTeamByIdAsync(int id);

        /// <summary>
        /// Cr�e une nouvelle �quipe
        /// </summary>
        Task<TeamDto> CreateTeamAsync(CreateTeamDto createTeamDto);

        /// <summary>
        /// Met � jour une �quipe existante
        /// </summary>
        Task<TeamDto> UpdateTeamAsync(int id, UpdateTeamDto updateTeamDto);

        /// <summary>
        /// Supprime une �quipe
        /// </summary>
        Task<bool> DeleteTeamAsync(int id);

        /// <summary>
        /// R�cup�re les joueurs d'une �quipe
        /// </summary>
        Task<IEnumerable<PlayerSummaryDto>> GetTeamPlayersAsync(int teamId);
    }
}