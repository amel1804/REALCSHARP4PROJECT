namespace BasketballLiveScore.DTOs.Team
{
    /// <summary>
    /// DTO pour une équipe
    /// </summary>
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Coach { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int PlayerCount { get; set; }
    }
}