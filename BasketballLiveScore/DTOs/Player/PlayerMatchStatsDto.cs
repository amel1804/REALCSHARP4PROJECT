using System;

namespace BasketballLiveScore.DTOs.Player
{
    /// <summary>
    /// DTO pour les statistiques d'un joueur dans un match
    /// </summary>
    public class PlayerMatchStatsDto
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int JerseyNumber { get; set; }
        public int Points { get; set; }
        public int Fouls { get; set; }
        public int MinutesPlayed { get; set; }
        public bool IsStarter { get; set; }
    }
}