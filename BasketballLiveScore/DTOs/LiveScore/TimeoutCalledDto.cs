using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.LiveScore
{
    /// <summary>
    /// DTO pour l'enregistrement d'un timeout
    /// </summary>
    public class TimeoutCalledDto
    {
        [Required]
        public int MatchId { get; set; }

        [Required]
        public int TeamId { get; set; }

        [Required]
        public int Quarter { get; set; }

        [Required]
        public int GameClockSeconds { get; set; }
    }
}