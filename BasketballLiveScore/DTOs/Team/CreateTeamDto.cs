using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.Team
{
    /// <summary>
    /// DTO pour la création d'une équipe
    /// </summary>
    public class CreateTeamDto
    {
        [Required(ErrorMessage = "Le nom de l'équipe est obligatoire")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Coach { get; set; } = string.Empty;
    }
}