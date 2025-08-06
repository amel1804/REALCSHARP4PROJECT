using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.Models
{
    /// <summary>
    /// Représente une équipe de basketball
    /// </summary>
    public class Team
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        // Ajout des propriétés manquantes
        [MaxLength(50)]
        public string Coach { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Liste des joueurs de l'équipe
        /// Navigation property comme vu dans ef_relations.cs
        /// </summary>
        public List<Player> Players { get; set; } = new();

        /// <summary>
        /// Matchs à domicile
        /// </summary>
        public List<Match> HomeMatches { get; set; } = new();

        /// <summary>
        /// Matchs à l'extérieur
        /// </summary>
        public List<Match> AwayMatches { get; set; } = new();
    }
}