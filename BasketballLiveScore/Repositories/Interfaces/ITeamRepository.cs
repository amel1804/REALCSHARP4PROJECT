using System.Collections.Generic;
using System.Threading.Tasks;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface du repository pour les équipes
    /// Suit le pattern Repository vu dans VideoGameManager
    /// </summary>
    public interface ITeamRepository : IRepository<Team>
    {
        /// <summary>
        /// Récupère une équipe avec ses joueurs
        /// </summary>
        Task<Team> GetTeamWithPlayersAsync(int teamId);

        /// <summary>
        /// Récupère les équipes d'une ville
        /// </summary>
        Task<IEnumerable<Team>> GetTeamsByCityAsync(string city);

        /// <summary>
        /// Vérifie si une équipe existe par son nom
        /// </summary>
        Task<bool> TeamExistsAsync(string teamName);

        // Ajout des méthodes manquantes
        Task<IEnumerable<Team>> GetAllAsync();
        Task<Team?> GetByIdAsync(int id);
        Task AddAsync(Team team);
    }
}