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
        /// Récupère tous les joueurs
        /// </summary>
        Task<IEnumerable<Player>> GetAllAsync();

        /// <summary>
        /// Récupère un joueur par son identifiant
        /// </summary>
        Task<Player?> GetByIdAsync(int id);

        /// <summary>
        /// Ajoute un nouveau joueur
        /// </summary>
        Task AddAsync(Player player);

        /// <summary>
        /// Récupère les joueurs d'une équipe
        /// </summary>
        Task<IEnumerable<Player>> GetPlayersByTeamAsync(int teamId);

        /// <summary>
        /// Récupère un joueur par son numéro et son équipe
        /// </summary>
        Task<Player> GetPlayerByNumberAndTeamAsync(int number, int teamId);

        /// <summary>
        /// Vérifie si un numéro est déjà utilisé dans une équipe
        /// </summary>
        Task<bool> IsNumberTakenInTeamAsync(int number, int teamId);
    }
}