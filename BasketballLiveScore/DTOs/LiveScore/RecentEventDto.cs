using System;

namespace BasketballLiveScore.DTOs.LiveScore
{
    /// <summary>
    /// DTO pour afficher un événement récent
    /// </summary>
    public class RecentEventDto
    {
        public DateTime EventTime { get; set; }
        public int Quarter { get; set; }
        public string GameClock { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PlayerName { get; set; }
        public string? TeamName { get; set; }
    }
}