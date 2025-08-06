using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.LiveScore
{
    /// <summary>
    /// DTO pour le changement de quart-temps
    /// </summary>
    public class ChangeQuarterDto
    {
        [Required]
        public int MatchId { get; set; }

        [Required]
        [Range(1, 10)]
        public int NewQuarter { get; set; }
    }
}