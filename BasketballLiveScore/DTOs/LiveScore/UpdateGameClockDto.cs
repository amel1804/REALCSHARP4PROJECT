using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.LiveScore
{
    /// <summary>
    /// DTO pour la mise � jour du chronom�tre
    /// </summary>
    public class UpdateGameClockDto
    {
        [Required]
        public int MatchId { get; set; }

        [Required]
        public int GameClockSeconds { get; set; }

        [Required]
        public bool IsRunning { get; set; }
    }
}