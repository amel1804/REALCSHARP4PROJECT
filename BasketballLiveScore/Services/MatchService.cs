using System;
using System.Collections.Generic;
using System.Linq;
using BasketballLiveScore.DTOs.Match;
using BasketballLiveScore.Models;
using BasketballLiveScore.Models.Enums;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service pour la gestion des matchs
    /// </summary>
    public class MatchService : IMatchService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MatchService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Crée un nouveau match avec les paramètres fournis
        /// </summary>
        public void CreateMatch(MatchDto matchDto)
        {
            if (matchDto == null)
                throw new ArgumentNullException(nameof(matchDto));

            // Récupération des équipes depuis la base
            var homeTeam = _unitOfWork.Teams.Find(t => t.Name == matchDto.HomeTeamName).FirstOrDefault();
            var awayTeam = _unitOfWork.Teams.Find(t => t.Name == matchDto.AwayTeamName).FirstOrDefault();

            if (homeTeam == null)
                throw new InvalidOperationException($"Home team '{matchDto.HomeTeamName}' not found");

            if (awayTeam == null)
                throw new InvalidOperationException($"Away team '{matchDto.AwayTeamName}' not found");

            var match = new Match
            {
                HomeTeamId = homeTeam.Id,
                AwayTeamId = awayTeam.Id,
                ScheduledDate = matchDto.ScheduledDate,
                Status = Enum.TryParse<BasketballLiveScore.Models.MatchStatus>(matchDto.Status, out var status) ? status : BasketballLiveScore.Models.MatchStatus.Scheduled,                // Exemple: définir d'autres propriétés par défaut ou calculées si besoin
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = 1 // TODO: Récupérer l'utilisateur courant
            };

            _unitOfWork.Matches.Add(match);
            _unitOfWork.Complete();
        }

        /// <summary>
        /// Récupère tous les matchs
        /// </summary>
        public List<Match> GetAllMatches()
        {
            return _unitOfWork.Matches.GetAll().ToList();
        }

        /// <summary>
        /// Récupère un match par son identifiant
        /// </summary>
        public Match GetMatchById(int id)
        {
            var match = _unitOfWork.Matches.GetById(id);
            if (match == null)
                throw new KeyNotFoundException($"Match with ID {id} not found");
            return match;
        }
    }
}
