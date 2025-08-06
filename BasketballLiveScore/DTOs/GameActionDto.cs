namespace BasketballLiveScore.DTOs
{
    public class GameActionDto
    {
        public int MatchId { get; set; }
        public string? PlayerName { get; set; }
        public string? ActionType { get; set; } // Exemple : "Basket", "Fault", "Substitution"
        public int Points { get; set; } // Points marqués (si applicable)
        public string? FaultType { get; set; } // Exemple : "P0", "P1", etc.
        public int Quarter { get; set; }
        public TimeSpan Timestamp { get; set; }
    }
}
