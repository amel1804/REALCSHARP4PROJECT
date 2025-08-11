using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BasketballLiveScore.Models.Events; // Import correct pour MatchEvent

namespace BasketballLiveScore.Models
{
    /// <summary>
    /// Représente un match de basketball
    /// Contient toutes les informations de configuration et d'état
    /// </summary>
    public class Match
    {
        public int Id { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public int NumberOfQuarters { get; set; } = 4;

        [Required]
        public int QuarterDurationMinutes { get; set; } = 10;

        [Required]
        public int TimeoutDurationSeconds { get; set; } = 60;

        // État du match
        public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // Scores
        public int HomeTeamScore { get; set; } = 0;
        public int AwayTeamScore { get; set; } = 0;

        // Quart-temps actuel
        public int CurrentQuarter { get; set; } = 0;

        // Temps restant en secondes
        public int RemainingTimeSeconds { get; set; } = 0;

        // Clés étrangères
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public int PreparedByUserId { get; set; }

        // Navigation properties
        public Team? HomeTeam { get; set; }
        public Team? AwayTeam { get; set; }
        public User? PreparedByUser { get; set; }

        // Collections
        public List<User> LiveEncoders { get; set; } = new();
        public List<MatchLineup> Lineups { get; set; } = new();

        // Ajout des propriétés manquantes
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int CreatedByUserId { get; set; }

        public virtual List<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();

        /// <summary>
        /// Liste des actions du jeu (si vous avez ce concept)
        /// Correspond à GameActions utilisé dans le DbContext
        /// </summary>
       
        public virtual List<GameAction> GameActions { get; set; } = new List<GameAction>();
        public List<MatchLineup> MatchLineups { get; set; } = new();

    }

    /// <summary>
    /// Énumération des statuts possibles d'un match
    /// </summary>
    /// /// <summary>
    /// Liste des événements du match
    /// Correspond à MatchEvents utilisé dans le DbContext
    /// </summary>

    public enum MatchStatus
    {
        Scheduled,
        InProgress,
        Halftime,
        Finished,
        Cancelled
    }
}