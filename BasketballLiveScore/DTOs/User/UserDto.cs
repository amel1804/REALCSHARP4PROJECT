using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.User
{
    /// <summary>
    /// DTO pour l'affichage d'un utilisateur
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }

}