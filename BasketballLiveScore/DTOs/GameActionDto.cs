using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs
{
    /// <summary>
    /// DTO pour enregistrer une action de jeu
    /// </summary>
    public class GameActionDto
    {
        [Required(ErrorMessage = "L'ID du match est obligatoire")]
        public int MatchId { get; set; }

        [Required(ErrorMessage = "L'ID du joueur est obligatoire")]
        public int PlayerId { get; set; }

        [Required(ErrorMessage = "Le type d'action est obligatoire")]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty;

        public int? Points { get; set; }

        [MaxLength(50)]
        public string? FaultType { get; set; }

        [Required(ErrorMessage = "Le numéro du quart-temps est obligatoire")]
        [Range(1, 4, ErrorMessage = "Le quart-temps doit être entre 1 et 4")]
        public int Quarter { get; set; }

        [Required(ErrorMessage = "Le temps de jeu est obligatoire")]
        public TimeSpan GameTime { get; set; }
    }
}