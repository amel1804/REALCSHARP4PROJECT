using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs
{
    /// <summary>
    /// DTO pour la mise à jour d'un utilisateur existant
    /// </summary>
    public class UserUpdateDto
    {
        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [MaxLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [MaxLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [MaxLength(100, ErrorMessage = "L'email ne peut pas dépasser 100 caractères")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public bool? IsActive { get; set; }

    }
}