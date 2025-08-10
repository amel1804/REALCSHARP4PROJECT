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

        /// <summary>
        /// Joueurs actuellement sur le terrain pour les deux équipes
        /// </summary>
        public TeamsOnCourtDto TeamsOnCourt { get; set; } = new();

        // Pour la compatibilité avec l'ancien code
        public List<PlayerOnCourtDto> HomeTeamPlayersOnCourt
        {
            get => TeamsOnCourt?.HomeTeamPlayers ?? new List<PlayerOnCourtDto>();
            set => TeamsOnCourt = new TeamsOnCourtDto
            {
                HomeTeamPlayers = value,
                AwayTeamPlayers = TeamsOnCourt?.AwayTeamPlayers ?? new List<PlayerOnCourtDto>()
            };
        }

        public List<PlayerOnCourtDto> AwayTeamPlayersOnCourt
        {
            get => TeamsOnCourt?.AwayTeamPlayers ?? new List<PlayerOnCourtDto>();
            set => TeamsOnCourt = new TeamsOnCourtDto
            {
                HomeTeamPlayers = TeamsOnCourt?.HomeTeamPlayers ?? new List<PlayerOnCourtDto>(),
                AwayTeamPlayers = value
            };
        }
    }
}