using System.Collections.Generic;

namespace BasketballLiveScore.DTOs.LiveScore
{
    /// <summary>
    /// DTO pour le statut en direct d'un match
    /// </summary>
    public class MatchLiveStatusDto
    {
        public int MatchId { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CurrentQuarter { get; set; }
        public int RemainingTimeSeconds { get; set; }
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }
        public List<PlayerOnCourtDto> HomeTeamPlayers { get; set; } = new();
        public List<PlayerOnCourtDto> AwayTeamPlayers { get; set; } = new();
        public int HomeTeamTimeoutsRemaining { get; set; }
        public int AwayTeamTimeoutsRemaining { get; set; }
    }
}