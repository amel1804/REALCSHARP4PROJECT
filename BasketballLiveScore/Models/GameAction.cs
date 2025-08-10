using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.Models
{
    public class GameAction
    {
        public int Id { get; set; }

        [Required]
        public int MatchId { get; set; }

        public int? PlayerId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty; // Basket, Fault, Substitution, Timeout

        public int Points { get; set; } // Pour les paniers (1, 2 ou 3)

        [MaxLength(10)]
        public string? FaultType { get; set; } // P0, P1, P2, P3

        public int? PlayerInId { get; set; } // Pour les changements
        public int? PlayerOutId { get; set; } // Pour les changements

        [Required]
        public int Quarter { get; set; }

        [Required]
        public TimeSpan GameTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Match? Match { get; set; }
        public virtual Player? Player { get; set; }
        public virtual Player? PlayerIn { get; set; }
        public virtual Player? PlayerOut { get; set; }
    }
}