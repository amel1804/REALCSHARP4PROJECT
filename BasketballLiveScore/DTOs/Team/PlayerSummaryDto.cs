namespace BasketballLiveScore.DTOs.Team
{
    /// <summary>
    /// DTO pour le résumé d'un joueur
    /// </summary>
    public class PlayerSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int JerseyNumber { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public bool IsStarter { get; set; } // Si le joueur fait partie du 5 de base
    }
}