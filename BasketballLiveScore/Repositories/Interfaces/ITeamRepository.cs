using System.Collections.Generic;
using System.Threading.Tasks;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface du repository pour les �quipes
    /// Suit le pattern Repository vu dans VideoGameManager
    /// </summary>
    public interface ITeamRepository : IRepository<Team>
    {
        /// <summary>
        /// R�cup�re une �quipe avec ses joueurs
        /// </summary>
        Task<Team> GetTeamWithPlayersAsync(int teamId);

        /// <summary>
        /// R�cup�re les �quipes d'une ville
        /// </summary>
        Task<IEnumerable<Team>> GetTeamsByCityAsync(string city);

        /// <summary>
        /// V�rifie si une �quipe existe par son nom
        /// </summary>
        Task<bool> TeamExistsAsync(string teamName);

        // Ajout des m�thodes manquantes
        Task<IEnumerable<Team>> GetAllAsync();
        Task<Team?> GetByIdAsync(int id);
        Task AddAsync(Team team);
    }
}