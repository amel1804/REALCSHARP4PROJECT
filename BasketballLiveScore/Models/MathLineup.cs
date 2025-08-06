using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.Models
{
    /// <summary>
    /// Représente la composition d'équipe pour un match
    /// Table de liaison entre Match et Player avec information supplémentaire
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