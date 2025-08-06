using System;
using System.Collections.Generic;
using BasketballLiveScore.DTOs;
using BasketballLiveScore.DTOs.Match;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service de gestion des matchs
    /// Suit le pattern vu dans IGameActionService
    /// </summary>
    public interface IMatchService
    {
        void CreateMatch(MatchDto matchDto);
        List<Match> GetAllMatches();
        Match GetMatchById(int id);
    }
}