using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.LiveScore
{
    /// <summary>
    /// DTO pour enregistrer un panier
    /// </summary>
    public class BasketScoreDto
    {
        [Required]
        public int PlayerId { get; set; }

        [Required]
        [Range(1, 3)]
        public int Points { get; set; }

        [Required]
        public int Quarter { get; set; }

        [Required]
        public TimeSpan GameTime { get; set; }
    }
}