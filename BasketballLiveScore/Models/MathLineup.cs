using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.Models
{
    /// <summary>
    /// Repr�sente la composition d'�quipe pour un match
    /// Table de liaison entre Match et Player avec information suppl�mentaire
    /// </summary>
    public class MatchLineup
    {
        public int Id { get; set; }

        [Required]
        public int MatchId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [Required]
        public int TeamId { get; set; }

        public bool IsStarter { get; set; }

        // Navigation properties
        public Match? Match { get; set; }
        public Player? Player { get; set; }
        public Team? Team { get; set; }
    }
}