using System;
using BasketballLiveScore.Models;
using BasketballLiveScore.Models.Enums;

namespace BasketballLiveScore.Models.Events
{
    /// <summary>
    /// Événement de changement de joueur
    /// </summary>
    public class SubstitutionEvent : MatchEvent
    {
        public int PlayerInId { get; set; }
        public int PlayerOutId { get; set; }

        // Navigation properties
        public Player PlayerIn { get; set; }
        public Player PlayerOut { get; set; }

        public override MatchEventType EventType => MatchEventType.Substitution;

        public override string GetDescription()
        {
            if (PlayerIn == null || PlayerOut == null)
            {
                return "Changement de joueur";
            }
            return $"{PlayerIn.FullName} remplace {PlayerOut.FullName}";
        }
    }
}
