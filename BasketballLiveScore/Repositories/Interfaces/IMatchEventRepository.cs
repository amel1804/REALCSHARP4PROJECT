using BasketballLiveScore.Models.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface pour le repository des �v�nements de match
    /// G�re toutes les actions qui se produisent pendant un match
    /// </summary>
    public interface IMatchEventRepository : IRepository<MatchEvent>
    {
        /// <summary>
        /// R�cup�re tous les �v�nements d'un match
        /// </summary>
        /// <param name="matchId">L'identifiant du match</param>
        /// <returns>La liste des �v�nements du match</returns>
        Task<IEnumerable<MatchEvent>> GetMatchEventsAsync(int matchId);

        /// <summary>
        /// R�cup�re les �v�nements d'un match pour un quart-temps sp�cifique
        /// </summary>
        /// <param name="matchId">L'identifiant du match</param>
        /// <param name="quarter">Le num�ro du quart-temps</param>
        /// <returns>La liste des �v�nements du quart-temps</returns>
        Task<IEnumerable<MatchEvent>> GetQuarterEventsAsync(int matchId, int quarter);

        /// <summary>
        /// R�cup�re les fautes d'un joueur dans un match
        /// </summary>
        /// <param name="matchId">L'identifiant du match</param>
        /// <param name="playerId">L'identifiant du joueur</param>
        /// <returns>La liste des fautes du joueur</returns>
        Task<IEnumerable<FoulEvent>> GetPlayerFoulsAsync(int matchId, int playerId);

        /// <summary>
        /// R�cup�re les points marqu�s par un joueur dans un match
        /// </summary>
        /// <param name="matchId">L'identifiant du match</param>
        /// <param name="playerId">L'identifiant du joueur</param>
        /// <returns>Le total des points marqu�s</returns>
        Task<int> GetPlayerPointsAsync(int matchId, int playerId);
    }
}