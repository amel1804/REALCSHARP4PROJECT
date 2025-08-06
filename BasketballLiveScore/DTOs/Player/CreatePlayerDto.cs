using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.Player
{
    /// <summary>
    /// DTO pour la création d'un joueur
    /// </summary>
    public class CreatePlayerDto
    {
        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numéro de maillot est obligatoire")]
        [Range(0, 99, ErrorMessage = "Le numéro doit être entre 0 et 99")]
        public int JerseyNumber { get; set; }

        [Required(ErrorMessage = "L'équipe est obligatoire")]
        public int TeamId { get; set; }
    }
}