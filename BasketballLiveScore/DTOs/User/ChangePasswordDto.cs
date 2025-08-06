
using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.User
{
    /// <summary>
    /// DTO pour le changement de mot de passe
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Le mot de passe actuel est obligatoire")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}