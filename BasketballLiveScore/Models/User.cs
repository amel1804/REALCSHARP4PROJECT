using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.Models
{
    /// <summary>
    /// Représente un utilisateur du système (encodeur ou administrateur)
    /// Implémente les propriétés nécessaires pour l'authentification
    /// </summary>
    public class User
    {
        // Constantes pour éviter les valeurs magiques
        public const int USERNAME_MAX_LENGTH = 50;
        public const int NAME_MAX_LENGTH = 50;
        public const int EMAIL_MAX_LENGTH = 100;
        public const int PASSWORD_MIN_LENGTH = 6;

        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        [MaxLength(USERNAME_MAX_LENGTH)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [MaxLength(NAME_MAX_LENGTH)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [MaxLength(NAME_MAX_LENGTH)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [MaxLength(EMAIL_MAX_LENGTH)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(PASSWORD_MIN_LENGTH)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le rôle est obligatoire")]
        [MaxLength(NAME_MAX_LENGTH)]
        public string Role { get; set; } = UserRoles.ENCODER;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Propriété calculée pour le nom complet
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        // Relations - Navigation properties
        public List<Match> PreparedMatches { get; set; } = new();
        public List<Match> EncodedMatches { get; set; } = new();

        /// <summary>
        /// Vérifie si l'utilisateur a un rôle spécifique
        /// </summary>
        public bool HasRole(string role)
        {
            return Role.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Vérifie si l'utilisateur est administrateur
        /// </summary>
        public bool IsAdministrator => HasRole(UserRoles.ADMINISTRATOR);
    }

    /// <summary>
    /// Constantes pour les rôles utilisateur
    /// </summary>
    public static class UserRoles
    {
        public const string ADMINISTRATOR = "Administrator";
        public const string ENCODER = "Encoder";
        public const string VIEWER = "Viewer";
    }
}