using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.Models
{
    /// <summary>
    /// Repr�sente une �quipe de basketball
    /// </summary>
    public class Team
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom de l'�quipe est obligatoire")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Coach { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Arena { get; set; } = string.Empty;

        [MaxLength(500)]
        public string LogoUrl { get; set; } = string.Empty;

        [MaxLength(10)]
        public string PrimaryColor { get; set; } = "#000000";

        [MaxLength(10)]
        public string SecondaryColor { get; set; } = "#FFFFFF";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual List<Player> Players { get; set; } = new();
        public virtual List<Match> HomeMatches { get; set; } = new();
        public virtual List<Match> AwayMatches { get; set; } = new();
        public virtual List<MatchLineup> MatchLineups { get; set; } = new();

        /// <summary>
        /// Obtient le nom complet de l'�quipe (Ville + Nom)
        /// </summary>
        public string FullName => string.IsNullOrWhiteSpace(City) ? Name : $"{City} {Name}";

        /// <summary>
        /// Obtient le nombre total de matchs jou�s
        /// </summary>
        public int TotalMatches => HomeMatches.Count + AwayMatches.Count;
    }
}