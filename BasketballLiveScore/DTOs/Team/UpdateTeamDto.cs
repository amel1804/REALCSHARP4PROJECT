using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.Team
{
    /// <summary>
    /// DTO pour la mise � jour d'une �quipe
    /// </summary>
    public class UpdateTeamDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Coach { get; set; }
    }
}