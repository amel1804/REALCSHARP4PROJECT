using System;
using System.ComponentModel.DataAnnotations;
using BasketballLiveScore.Models.Enums;

namespace BasketballLiveScore.Models.Events
{
    /// <summary>
    /// Événement de faute
    /// </summary>
    public class FoulEvent : MatchEvent
    {
        [Required]
        [MaxLength(10)]
        public string FoulType { get; set; } = string.Empty; // P0, P1, P2, P3

        public override MatchEventType EventType => MatchEventType.Foul;

        public override string GetDescription()
        {
            if (Player == null)
            {
                return $"Faute {FoulType}";
            }
            return $"Faute {FoulType} de {Player.FullName}";
        }
    }
}
