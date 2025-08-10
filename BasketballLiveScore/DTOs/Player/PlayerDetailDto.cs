using System;
using System.Collections.Generic;

namespace BasketballLiveScore.DTOs.Player
{
    /// <summary>
    /// DTO détaillé pour un joueur avec toutes ses informations
    /// </summary>
    public class PlayerDetailDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int JerseyNumber { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Statistiques globales du joueur
        /// </summary>
        public PlayerStatsDto GlobalStats { get; set; } = new();

        /// <summary>
        /// Historique des matchs joués
        /// </summary>
        public List<PlayerMatchHistoryDto> MatchHistory { get; set; } = new();
    }

    /// <summary>
    /// DTO pour les statistiques globales d'un joueur
    /// </summary>
    public class PlayerStatsDto
    {
        public int TotalMatches { get; set; }
        public int TotalPoints { get; set; }
        public int TotalFouls { get; set; }
        public double AveragePointsPerMatch { get; set; }
        public int TotalMinutesPlayed { get; set; }
    }
    /// <summary>
    /// DTO pour l'historique des matchs d'un joueur
    /// </summary>
    public class PlayerMatchHistoryDto
    {
        public int MatchId { get; set; }
        public DateTime MatchDate { get; set; }
        public string OpponentTeamName { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public PlayerStatsDto Stats { get; set; } = new();
    }
}