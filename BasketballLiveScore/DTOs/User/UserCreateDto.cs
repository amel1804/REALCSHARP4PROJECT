
using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs
{
    /// <summary>
    /// DTO pour la création d'un nouvel utilisateur
    /// </summary>
    public class UserCreateDto
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

        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        [MaxLength(100, ErrorMessage = "Le nom d'utilisateur ne peut pas dépasser 100 caractères")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Le nom d'utilisateur ne peut contenir que des lettres, chiffres et underscores")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}