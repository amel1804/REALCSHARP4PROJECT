using System.Collections.Generic;
using System.Threading.Tasks;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface du repository pour les joueurs
    /// </summary>
    public interface IPlayerRepository : IRepository<Player>
    {
        /// <summary>
        /// R�cup�re tous les joueurs
        /// </summary>
        Task<IEnumerable<Player>> GetAllAsync();

        /// <summary>
        /// R�cup�re un joueur par son identifiant
        /// </summary>
        Task<Player?> GetByIdAsync(int id);

        /// <summary>
        /// Ajoute un nouveau joueur
        /// </summary>
        Task AddAsync(Player player);

        /// <summary>
        /// R�cup�re les joueurs d'une �quipe
        /// </summary>
        Task<IEnumerable<Player>> GetPlayersByTeamAsync(int teamId);

        /// <summary>
        /// R�cup�re un joueur par son num�ro et son �quipe
        /// </summary>
        Task<Player> GetPlayerByNumberAndTeamAsync(int number, int teamId);

        /// <summary>
        /// V�rifie si un num�ro est d�j� utilis� dans une �quipe
        /// </summary>
        Task<bool> IsNumberTakenInTeamAsync(int number, int teamId);
    }
}