using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.Match
{
    /// <summary>
    /// DTO pour la création et configuration d'un match
    /// </summary>
    public class MatchSetupDto
    {
        [Required]
        public int HomeTeamId { get; set; }

        [Required]
        public int AwayTeamId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [Range(1, 10)]
        public int NumberOfQuarters { get; set; } = 4;

        [Required]
        [Range(5, 20)]
        public int QuarterDurationMinutes { get; set; } = 10;

        [Required]
        [Range(30, 120)]
        public int TimeoutDurationSeconds { get; set; } = 60;

        [Required]
        public List<int> HomeTeamStartingLineup { get; set; } = new();

        [Required]
        public List<int> AwayTeamStartingLineup { get; set; } = new();
    }
}