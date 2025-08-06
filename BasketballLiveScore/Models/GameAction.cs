using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.Models
{
    /// <summary>
    /// Représente une action de jeu durant un match
    /// Basé sur le pattern Entity vu dans les codes de cours
    /// </summary>
    public class GameAction
    {
        public int Id { get; set; }

        [Required]
        public int MatchId { get; set; }

        [MaxLength(100)]
        public string? PlayerName { get; set; }

        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty;

        public int Points { get; set; }

        [MaxLength(10)]
        public string? FaultType { get; set; }

        [Required]
        public int Quarter { get; set; }

        [Required]
        public TimeSpan Timestamp { get; set; }

        // Navigation property - comme dans ef_relations.cs
        public Match? Match { get; set; }
    }
}