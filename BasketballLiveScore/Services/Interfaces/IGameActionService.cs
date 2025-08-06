using BasketballLiveScore.DTOs;
using BasketballLiveScore.Models;
using System.Collections.Generic;

namespace BasketballLiveScore.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service de gestion des actions de jeu
    /// Suit le pattern vu dans les notes de cours sur les interfaces
    /// </summary>
    public interface IGameActionService
    {
        /// <summary>
        /// Enregistre une action de jeu
        /// </summary>
        /// <param name="actionDto">Les données de l'action à enregistrer</param>
        void RecordAction(GameActionDto actionDto);

        /// <summary>
        /// Récupère toutes les actions pour un match donné
        /// </summary>
        /// <param name="matchId">L'identifiant du match</param>
        /// <returns>La liste des actions du match</returns>
        List<GameAction> GetActionsForMatch(int matchId);
    }
}