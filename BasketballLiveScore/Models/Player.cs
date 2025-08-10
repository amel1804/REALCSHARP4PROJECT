using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.Models
{
    /// <summary>
    /// Représente un joueur de basketball
    /// </summary>
    public class Player
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Range(0, 99)]
        public int JerseyNumber { get; set; } // Utilisez JerseyNumber au lieu de Number

        // Clé étrangère
        public int TeamId { get; set; }

        // Navigation property
        public Team? Team { get; set; }

        // Propriété calculée pour le nom complet
        public string FullName => $"{FirstName} {LastName}";
        public List<MatchLineup> MatchLineups { get; set; } = new List<MatchLineup>();


    }
}