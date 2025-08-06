using System;
using BasketballLiveScore.Models;
using BasketballLiveScore.Models.Enums;

namespace BasketballLiveScore.Models.Events
{
    /// <summary>
    /// Événement de temps mort
    /// </summary>
    public class TimeoutEvent : MatchEvent
    {
        /// <summary>
        /// Identifiant de l'équipe qui a pris le temps mort
        /// CORRECTION: Ajout de la clé étrangère selon les conventions EF vues en cours
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// Propriété de navigation vers l'équipe
        /// Pattern vu dans les exemples de cours (Game/GameGenre)
        /// </summary>
        public virtual Team Team { get; set; }

        public override MatchEventType EventType => MatchEventType.Timeout;

        public override string GetDescription()
        {
            if (Team == null)
            {
                return "Temps mort";
            }
            return $"Temps mort pour {Team.Name}";
        }
    }
}