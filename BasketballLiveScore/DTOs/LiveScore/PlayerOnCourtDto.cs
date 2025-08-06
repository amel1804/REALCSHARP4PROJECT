using System;

namespace BasketballLiveScore.DTOs.LiveScore
{
    /// <summary>
    /// DTO pour un joueur sur le terrain
    /// </summary>
    public class PlayerOnCourtDto
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int JerseyNumber { get; set; }
        public int PersonalFouls { get; set; }
        public int Points { get; set; }
    }
}