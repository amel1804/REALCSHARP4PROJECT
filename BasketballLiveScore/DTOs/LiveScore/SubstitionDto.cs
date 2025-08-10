
using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.Player
{
    /// <summary>
    /// DTO pour effectuer une substitution de joueur
    /// </summary>
    public class SubstitutionDto
    {
        [Required(ErrorMessage = "L'identifiant du match est obligatoire")]
        public int MatchId { get; set; }

        [Required(ErrorMessage = "Le joueur entrant est obligatoire")]
        public int PlayerInId { get; set; }

        [Required(ErrorMessage = "Le joueur sortant est obligatoire")]
        public int PlayerOutId { get; set; }

        [Required(ErrorMessage = "Le quart-temps est obligatoire")]
        [Range(1, 4, ErrorMessage = "Le quart-temps doit être entre 1 et 4")]
        public int Quarter { get; set; }

        [Required(ErrorMessage = "Le temps de jeu est obligatoire")]
        public TimeSpan GameTime { get; set; }

        /// <summary>
        /// Raison de la substitution (optionnel)
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }
}