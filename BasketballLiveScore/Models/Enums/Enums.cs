namespace BasketballLiveScore.Models.Enums
{
    /// <summary>
    /// Types d'événements possibles dans un match
    /// </summary>
    public enum MatchEventType
    {
        Basket,         
        Foul,           
        Substitution,   
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