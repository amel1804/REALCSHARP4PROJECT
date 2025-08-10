using System;
using BasketballLiveScore.Models;
using BasketballLiveScore.Models.Enums;

namespace BasketballLiveScore.Models.Events
{
    /// <summary>
    /// �v�nement de temps mort
    /// </summary>
    public class TimeoutEvent : MatchEvent
    {
        /// <summary>
        /// Identifiant de l'�quipe qui a pris le temps mort
        /// CORRECTION: Ajout de la cl� �trang�re selon les conventions EF vues en cours
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// Propri�t� de navigation vers l'�quipe
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