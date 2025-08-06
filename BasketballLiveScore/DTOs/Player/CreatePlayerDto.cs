using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.Player
{
    /// <summary>
    /// DTO pour la cr�ation d'un joueur
    /// </summary>
    public class CreatePlayerDto
    {
        [Required(ErrorMessage = "Le pr�nom est obligatoire")]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le num�ro de maillot est obligatoire")]
        [Range(0, 99, ErrorMessage = "Le num�ro doit �tre entre 0 et 99")]
        public int JerseyNumber { get; set; }

        [Required(ErrorMessage = "L'�quipe est obligatoire")]
        public int TeamId { get; set; }
    }
}