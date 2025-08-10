// BasketballLiveScore/DTOs/User/UserRegistrationDto.cs
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.User
{
    /// <summary>
    /// DTO pour l'enregistrement d'un nouvel utilisateur
    /// Respecte les conventions C# et les patterns vus en cours
    /// </summary>
    public class UserRegistrationDto
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le rôle est obligatoire")]
        [RegularExpression("^(Administrator|Encoder|Viewer)$", ErrorMessage = "Rôle invalide")]
        public string Role { get; set; } = "Encoder";
    }
}