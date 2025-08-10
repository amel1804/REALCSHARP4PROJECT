namespace BasketballLiveScore.DTOs.Player
{
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
