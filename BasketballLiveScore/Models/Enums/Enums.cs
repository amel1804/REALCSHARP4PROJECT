namespace BasketballLiveScore.Models.Enums
{
    /// <summary>
    /// Types d'événements possibles dans un match
    /// </summary>
    public enum MatchEventType
    {
        Basket,         // au lieu de BasketScored
        Foul,           // au lieu de FoulCommitted
        Substitution,   // au lieu de PlayerSubstitution
        Timeout,
        QuarterStart,
        QuarterEnd
    }

    /// <summary>
    /// Types de paniers
    /// </summary>
    public enum BasketType
    {
        FreeThrow = 1,
        TwoPoints = 2,
        ThreePoints = 3
    }

    /// <summary>
    /// Statuts possibles d'un match
    /// </summary>
    public enum MatchStatus
    {
        Scheduled,
        InProgress,
        Finished,
        Cancelled
    }
}