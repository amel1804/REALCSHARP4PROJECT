using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.Player
{
    /// <summary>
    /// DTO pour la mise à jour d'un joueur
    /// </summary>
    public class UpdatePlayerDto
    {
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [Range(0, 99)]
        public int? JerseyNumber { get; set; }

        public int? TeamId { get; set; }
    }
}