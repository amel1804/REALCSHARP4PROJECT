using System;
using System.Collections.Generic;
using BasketballLiveScore.DTOs.Team;
using BasketballLiveScore.DTOs.Player;

namespace BasketballLiveScore.DTOs.Match
{
    /// <summary>
    /// DTO détaillé pour un match avec toutes les informations
    /// </summary>
    public class MatchDetailDto
    {
        public int Id { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TeamDto? HomeTeam { get; set; }
        public TeamDto? AwayTeam { get; set; }
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CurrentQuarter { get; set; }
        public int RemainingTimeSeconds { get; set; }
        public int NumberOfQuarters { get; set; }
        public int QuarterDurationMinutes { get; set; }
        public List<PlayerSummaryDto> HomeTeamPlayers { get; set; } = new();
        public List<PlayerSummaryDto> AwayTeamPlayers { get; set; } = new();
        public List<RecentEventDto> RecentEvents { get; set; } = new();
    }
}