using System;
using System.Collections.Generic;

namespace BasketballLiveScore.DTOs.Team
{
    /// <summary>
    /// DTO détaillé pour une équipe avec ses joueurs
    /// </summary>
    public class TeamDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Coach { get; set; } = string.Empty;
        public List<PlayerSummaryDto> Players { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}