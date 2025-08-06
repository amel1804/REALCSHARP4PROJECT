using System;
using System.Collections.Generic;
using BasketballLiveScore.DTOs.Player;

namespace BasketballLiveScore.DTOs.Match
{
    /// <summary>
    /// DTO pour le résumé d'un match terminé
    /// </summary>
    public class MatchSummaryDto
    {
        public int Id { get; set; }
        public DateTime MatchDate { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string AwayTeamName { get; set; } = string.Empty;
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }
        public string Duration { get; set; } = string.Empty;
        public List<PlayerMatchStatsDto> HomeTeamStats { get; set; } = new();
        public List<PlayerMatchStatsDto> AwayTeamStats { get; set; } = new();
    }
}