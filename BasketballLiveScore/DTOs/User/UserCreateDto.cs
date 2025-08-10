using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.User
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

        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        [MinLength(3, ErrorMessage = "Le nom d'utilisateur doit contenir au moins 3 caract�res")]
        [MaxLength(50, ErrorMessage = "Le nom d'utilisateur ne peut pas d�passer 50 caract�res")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [MaxLength(100, ErrorMessage = "L'email ne peut pas d�passer 100 caract�res")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caract�res")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le r�le est obligatoire")]
        public string Role { get; set; } = "Viewer";
    }
}