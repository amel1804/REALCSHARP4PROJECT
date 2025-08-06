using System;
using BasketballLiveScore.Models.Enums;

namespace BasketballLiveScore.Models.Events
{
    /// <summary>
    /// Événement représentant un panier marqué pendant le match
    /// </summary>
    public class BasketScoredEvent : MatchEvent
    {
        public override MatchEventType EventType => MatchEventType.Basket;

        /// <summary>
        /// Type de panier (1, 2 ou 3 points)
        /// </summary>
        public BasketType BasketType { get; set; }

        /// <summary>
        /// Indique si c'est un lancer franc
        /// </summary>
        public bool IsFreeThrow { get; set; }

        public BasketScoredEvent()
        {
            // Calculer automatiquement les points à la création
            CalculatePoints();
        }

        /// <summary>
        /// Calcule automatiquement les points selon le type de panier
        /// Pattern Template Method vu en cours
        /// </summary>
        public void CalculatePoints()
        {
            Points = BasketType switch
            {
                BasketType.FreeThrow => 1,
                BasketType.TwoPoints => 2,
                BasketType.ThreePoints => 3,
                _ => 0
            };
        }

        /// <summary>
        /// Description textuelle de l'événement
        /// Implémentation de la méthode abstraite selon le pattern Template Method
        /// </summary>
        public override string GetDescription()
        {
            if (Player == null)
            {
                return $"Panier de {Points} points";
            }
            return $"{Player.FullName} marque {Points} points";
        }
    }
}