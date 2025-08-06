using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.LiveScore
{
    /// <summary>
    /// DTO pour enregistrer un changement de joueur
    /// </summary>
    public class PlayerSubstitutionDto
    {
        [Required]
        public int PlayerInId { get; set; }

        [Required]
        public int PlayerOutId { get; set; }

        [Required]
        public int Quarter { get; set; }

        [Required]
        public TimeSpan GameTime { get; set; }
    }
}