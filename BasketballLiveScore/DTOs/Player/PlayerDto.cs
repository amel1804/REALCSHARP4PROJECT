using System;

namespace BasketballLiveScore.DTOs.Player
{
    /// <summary>
    /// DTO pour l'affichage d'un joueur
    /// </summary>
    public class PlayerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int JerseyNumber { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
    }
}