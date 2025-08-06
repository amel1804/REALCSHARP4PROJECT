
using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs
{
    /// <summary>
    /// DTO pour la cr�ation d'un nouvel utilisateur
    /// </summary>
    public class UserCreateDto
    {
        [Required(ErrorMessage = "Le pr�nom est obligatoire")]
        [MaxLength(50, ErrorMessage = "Le pr�nom ne peut pas d�passer 50 caract�res")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [MaxLength(50, ErrorMessage = "Le nom ne peut pas d�passer 50 caract�res")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [MaxLength(100, ErrorMessage = "L'email ne peut pas d�passer 100 caract�res")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        [MaxLength(100, ErrorMessage = "Le nom d'utilisateur ne peut pas d�passer 100 caract�res")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Le nom d'utilisateur ne peut contenir que des lettres, chiffres et underscores")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caract�res")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}