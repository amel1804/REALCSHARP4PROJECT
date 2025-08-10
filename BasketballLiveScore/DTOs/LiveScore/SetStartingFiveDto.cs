
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.Player
{
    /// <summary>
    /// DTO pour d�finir les 5 joueurs de base d'une �quipe
    /// </summary>
    public class SetStartingFiveDto
    {
        [Required(ErrorMessage = "L'identifiant du match est obligatoire")]
        public int MatchId { get; set; }

        [Required(ErrorMessage = "L'identifiant de l'�quipe est obligatoire")]
        public int TeamId { get; set; }

        [Required(ErrorMessage = "Les joueurs sont obligatoires")]
        [MinLength(5, ErrorMessage = "Il faut exactement 5 joueurs")]
        [MaxLength(5, ErrorMessage = "Il faut exactement 5 joueurs")]
        public List<int> PlayerIds { get; set; } = new();

        /// <summary>
        /// Valide que exactement 5 joueurs sont s�lectionn�s
        /// </summary>
        public bool IsValid()
        {
            return PlayerIds != null && PlayerIds.Count == 5;
        }
    }
    }