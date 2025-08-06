using System;

namespace BasketballLiveScore.DTOs.Match
{
    /// <summary>
    /// DTO de base pour un match
    /// </summary>
    public class MatchDto
    {
        public int Id { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string AwayTeamName { get; set; } = string.Empty;
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CurrentQuarter { get; set; }
    }
}