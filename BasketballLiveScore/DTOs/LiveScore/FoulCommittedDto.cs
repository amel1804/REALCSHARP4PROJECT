using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.LiveScore
{
    /// <summary>
    /// DTO pour enregistrer une faute
    /// </summary>
    public class FoulCommittedDto
    {
        [Required]
        public int PlayerId { get; set; }

        [Required]
        [MaxLength(10)]
        public string FoulType { get; set; } = string.Empty;

        [Required]
        public int Quarter { get; set; }

        [Required]
        public TimeSpan GameTime { get; set; }
    }
}