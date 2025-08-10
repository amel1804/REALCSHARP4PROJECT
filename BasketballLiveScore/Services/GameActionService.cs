using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs;
using BasketballLiveScore.Models;
using BasketballLiveScore.Data;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BasketballLiveScore.Services
{
    public class GameActionService : IGameActionService
    {
        private readonly BasketballDbContext _context;

        public GameActionService(BasketballDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Enregistre une action de jeu à partir d'un DTO
        /// </summary>
        public void RecordAction(GameActionDto actionDto)
        {
            if (actionDto == null)
                throw new ArgumentNullException(nameof(actionDto));

            // Validation simple
            if (actionDto.PlayerId <= 0)
                throw new ArgumentException("L'ID du joueur doit être supérieur à zéro.");

            if (string.IsNullOrWhiteSpace(actionDto.ActionType))
                throw new ArgumentException("Le type d'action est obligatoire.");

            if (actionDto.Quarter < 1 || actionDto.Quarter > 4)
                throw new ArgumentException("Le quart-temps doit être entre 1 et 4.");

            // Création de l'entité à partir du DTO
            var gameAction = new GameAction
            {
                MatchId = actionDto.MatchId,
                PlayerId = actionDto.PlayerId,
                ActionType = actionDto.ActionType,
                Points = actionDto.Points ?? 0,
                FaultType = actionDto.FaultType,
                Quarter = actionDto.Quarter,
                GameTime = actionDto.GameTime
            };

            _context.GameActions.Add(gameAction);
            _context.SaveChanges(); // Version synchrone pour correspondre à l'interface
        }

        /// <summary>
        /// Récupère toutes les actions pour un match donné
        /// </summary>
        /// <param name="matchId">L'identifiant du match</param>
        /// <returns>La liste des actions du match</returns>
        public List<GameAction> GetActionsForMatch(int matchId)
        {
            return _context.GameActions
                .Where(a => a.MatchId == matchId)
                .OrderBy(a => a.Quarter)
                .ThenBy(a => a.GameTime)
                .ToList();
        }

        // Si vous voulez garder une version async, vous pouvez l'ajouter en plus
        // (mais elle ne sera pas dans l'interface)
        public async Task RecordActionAsync(GameActionDto actionDto)
        {
            if (actionDto == null)
                throw new ArgumentNullException(nameof(actionDto));

            // Validation simple
            if (actionDto.PlayerId <= 0)
                throw new ArgumentException("L'ID du joueur doit être supérieur à zéro.");

            if (string.IsNullOrWhiteSpace(actionDto.ActionType))
                throw new ArgumentException("Le type d'action est obligatoire.");

            if (actionDto.Quarter < 1 || actionDto.Quarter > 4)
                throw new ArgumentException("Le quart-temps doit être entre 1 et 4.");

            // Création de l'entité à partir du DTO
            var gameAction = new GameAction
            {
                MatchId = actionDto.MatchId,
                PlayerId = actionDto.PlayerId,
                ActionType = actionDto.ActionType,
                Points = actionDto.Points ?? 0,
                FaultType = actionDto.FaultType,
                Quarter = actionDto.Quarter,
                GameTime = actionDto.GameTime
            };

            _context.GameActions.Add(gameAction);
            await _context.SaveChangesAsync();
        }
    }
}