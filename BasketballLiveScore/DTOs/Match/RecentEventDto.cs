using System;

namespace BasketballLiveScore.DTOs.Match
{
    /// <summary>
    /// DTO pour l'affichage des événements récents
    /// </summary>
    public class RecentEventDto
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quarter { get; set; }
        public string GameTime { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}