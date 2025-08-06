using System;
using System.Collections.Generic;

namespace BasketballLiveScore.DTOs.LiveScore
{
    /// <summary>
    /// DTO pour les mises à jour du score en temps réel
    /// </summary>
    public class LiveScoreUpdateDto
    {
        public int MatchId { get; set; }
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }
        public int CurrentQuarter { get; set; }
        public int RemainingTimeSeconds { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<PlayerOnCourtDto> HomeTeamPlayersOnCourt { get; set; } = new();
        public List<PlayerOnCourtDto> AwayTeamPlayersOnCourt { get; set; } = new();
    }
}