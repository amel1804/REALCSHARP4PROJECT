using System;
using System.Collections.Generic;

namespace BasketballLiveScore.DTOs.LiveScore
{
    /// <summary>
    /// DTO principal pour l'affichage du score en direct
    /// </summary>
    public class LiveScoreDto
    {
        public int MatchId { get; set; }

        /// <summary>
        /// Informations sur le match
        /// </summary>
        public MatchInfoDto MatchInfo { get; set; } = new();

        /// <summary>
        /// Score actuel
        /// </summary>
        public ScoreDto CurrentScore { get; set; } = new();

        /// <summary>
        /// État du chronomètre
        /// </summary>
        public GameClockDto GameClock { get; set; } = new();

        /// <summary>
        /// Joueurs actuellement sur le terrain
        /// </summary>
        public TeamsOnCourtDto TeamsOnCourt { get; set; } = new();

        /// <summary>
        /// Événements récents
        /// </summary>
        public List<RecentEventDto> RecentEvents { get; set; } = new();

        /// <summary>
        /// Statistiques du quart-temps actuel
        /// </summary>
        public QuarterStatsDto CurrentQuarterStats { get; set; } = new();
    }

    /// <summary>
    /// DTO pour les informations générales du match
    /// </summary>
    public class MatchInfoDto
    {
        public int Id { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string AwayTeamName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO pour le score
    /// </summary>
    public class ScoreDto
    {
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }

        /// <summary>
        /// Scores par quart-temps
        /// </summary>
        public List<QuarterScoreDto> QuarterScores { get; set; } = new();
    }

    /// <summary>
    /// DTO pour le score d'un quart-temps
    /// </summary>
    public class QuarterScoreDto
    {
        public int Quarter { get; set; }
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }
    }

    /// <summary>
    /// DTO pour l'état du chronomètre
    /// </summary>
    public class GameClockDto
    {
        public int CurrentQuarter { get; set; }
        public int RemainingSeconds { get; set; }
        public bool IsRunning { get; set; }
        public string FormattedTime => $"{RemainingSeconds / 60:D2}:{RemainingSeconds % 60:D2}";
    }

    /// <summary>
    /// DTO pour les équipes sur le terrain
    /// </summary>
    public class TeamsOnCourtDto
    {
        public List<PlayerOnCourtDto> HomeTeamPlayers { get; set; } = new();
        public List<PlayerOnCourtDto> AwayTeamPlayers { get; set; } = new();
    }

    /// <summary>
    /// DTO pour les statistiques du quart-temps
    /// </summary>
    public class QuarterStatsDto
    {
        public int Quarter { get; set; }
        public int HomeTeamPoints { get; set; }
        public int AwayTeamPoints { get; set; }
        public int HomeTeamFouls { get; set; }
        public int AwayTeamFouls { get; set; }
        public int HomeTeamTimeouts { get; set; }
        public int AwayTeamTimeouts { get; set; }
    }
}