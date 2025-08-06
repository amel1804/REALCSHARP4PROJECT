using BasketballLiveScore.Models.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface pour le repository des événements de match
    /// Gère toutes les actions qui se produisent pendant un match
    /// </summary>
    public interface IMatchEventRepository : IRepository<MatchEvent>
    {
        /// <summary>
        /// Récupère tous les événements d'un match
        /// </summary>
        /// <param name="matchId">L'identifiant du match</param>
        /// <returns>La liste des événements du match</returns>
        Task<IEnumerable<MatchEvent>> GetMatchEventsAsync(int matchId);

        /// <summary>
        /// Récupère les événements d'un match pour un quart-temps spécifique
        /// </summary>
        /// <param name="matchId">L'identifiant du match</param>
        /// <param name="quarter">Le numéro du quart-temps</param>
        /// <returns>La liste des événements du quart-temps</returns>
        Task<IEnumerable<MatchEvent>> GetQuarterEventsAsync(int matchId, int quarter);

        /// <summary>
        /// Récupère les fautes d'un joueur dans un match
        /// </summary>
        /// <param name="matchId">L'identifiant du match</param>
        /// <param name="playerId">L'identifiant du joueur</param>
        /// <returns>La liste des fautes du joueur</returns>
        Task<IEnumerable<FoulEvent>> GetPlayerFoulsAsync(int matchId, int playerId);

        /// <summary>
        /// Récupère les points marqués par un joueur dans un match
        /// </summary>
        /// <param name="matchId">L'identifiant du match</param>
        /// <param name="playerId">L'identifiant du joueur</param>
        /// <returns>Le total des points marqués</returns>
        Task<int> GetPlayerPointsAsync(int matchId, int playerId);
    }
}