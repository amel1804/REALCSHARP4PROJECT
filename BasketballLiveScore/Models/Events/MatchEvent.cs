using System;
using System.ComponentModel.DataAnnotations;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Models.Events
{
    /// <summary>
    /// Classe de base pour tous les événements d'un match
    /// Utilise le pattern TPH (Table Per Hierarchy)
    /// </summary>
    public class MatchEvent
    {
        public int Id { get; set; }

        [Required]
        public int MatchId { get; set; }

        public int? PlayerId { get; set; }

        [Required]
        public int Quarter { get; set; }

        [Required]
        public TimeSpan GameTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedById { get; set; }

        // Navigation properties
        public virtual Match? Match { get; set; }
        public virtual Player? Player { get; set; }
        public virtual User? CreatedBy { get; set; }
    }

    /// <summary>
    /// Événement de faute
    /// </summary>
    public class FoulEvent : MatchEvent
    {
        [Required]
        [MaxLength(10)]
        public string FoulType { get; set; } = string.Empty; // P0, P1, P2, P3, T, U, D

        public int? FouledPlayerId { get; set; }

        public virtual Player? FouledPlayer { get; set; }

        /// <summary>
        /// Nombre de lancers francs accordés suite à cette faute
        /// </summary>
        public int FreeThrowsAwarded { get; set; } = 0;
    }

    /// <summary>
    /// Événement de changement de joueur
    /// </summary>
    public class SubstitutionEvent : MatchEvent
    {
        [Required]
        public int PlayerInId { get; set; }

        [Required]
        public int PlayerOutId { get; set; }

        public virtual Player? PlayerIn { get; set; }
        public virtual Player? PlayerOut { get; set; }
    }

    /// <summary>
    /// Événement de timeout
    /// </summary>
    public class TimeoutEvent : MatchEvent
    {
        [Required]
        public int TeamId { get; set; }

        /// <summary>
        /// Type de timeout (complet ou 20 secondes)
        /// </summary>
        [MaxLength(20)]
        public string TimeoutType { get; set; } = "Full"; // Full, Twenty

        /// <summary>
        /// Temps restant au chronomètre lors du timeout (en secondes)
        /// </summary>
        public int GameClockSeconds { get; set; }

        public virtual Team? Team { get; set; }
    }

    /// <summary>
    /// Événement de panier marqué (optionnel, peut être géré via GameAction)
    /// </summary>
    public class BasketEvent : MatchEvent
    {
        [Required]
        [Range(1, 3)]
        public int Points { get; set; }

        /// <summary>
        /// Joueur ayant fait la passe décisive
        /// </summary>
        public int? AssistPlayerId { get; set; }

        /// <summary>
        /// Type de tir
        /// </summary>
        [MaxLength(50)]
        public string ShotType { get; set; } = string.Empty; // Layup, Dunk, Jump Shot, Three Pointer, Free Throw

        /// <summary>
        /// Distance du tir (en mètres)
        /// </summary>
        public decimal? ShotDistance { get; set; }

        public virtual Player? AssistPlayer { get; set; }
    }

    /// <summary>
    /// Événement de rebond
    /// </summary>
    public class ReboundEvent : MatchEvent
    {
        /// <summary>
        /// Type de rebond
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ReboundType { get; set; } = string.Empty; // Offensive, Defensive
    }

    /// <summary>
    /// Événement de vol de balle
    /// </summary>
    public class StealEvent : MatchEvent
    {
        /// <summary>
        /// Joueur qui a perdu la balle
        /// </summary>
        public int? TurnoverPlayerId { get; set; }

        public virtual Player? TurnoverPlayer { get; set; }
    }

    /// <summary>
    /// Événement de perte de balle
    /// </summary>
    public class TurnoverEvent : MatchEvent
    {
        /// <summary>
        /// Type de perte de balle
        /// </summary>
        [MaxLength(50)]
        public string TurnoverType { get; set; } = string.Empty; // Travel, Double Dribble, Out of Bounds, Bad Pass, etc.

        /// <summary>
        /// Joueur ayant récupéré la balle (si applicable)
        /// </summary>
        public int? StealPlayerId { get; set; }

        public virtual Player? StealPlayer { get; set; }
    }

    /// <summary>
    /// Événement de contre
    /// </summary>
    public class BlockEvent : MatchEvent
    {
        /// <summary>
        /// Joueur dont le tir a été contré
        /// </summary>
        [Required]
        public int BlockedPlayerId { get; set; }

        public virtual Player? BlockedPlayer { get; set; }
    }
}