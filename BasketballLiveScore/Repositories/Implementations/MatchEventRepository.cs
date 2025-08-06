using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BasketballLiveScore.Data;
using BasketballLiveScore.Models.Events;
using BasketballLiveScore.Repositories.Interfaces;

namespace BasketballLiveScore.Repositories.Implementations
{
    /// <summary>
    /// Repository pour la gestion des événements de match
    /// Implémente le pattern Repository spécifique aux événements
    /// </summary>
    public class MatchEventRepository : IMatchEventRepository
    {
        private readonly BasketballDbContext _context;
        private readonly DbSet<MatchEvent> _dbSet;

        public MatchEventRepository(BasketballDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<MatchEvent>();
        }

        /// <summary>
        /// Récupère un événement par son identifiant
        /// </summary>
        public MatchEvent GetById(int id)
        {
            return _dbSet.Find(id);
        }

        /// <summary>
        /// Récupère tous les événements
        /// </summary>
        public IEnumerable<MatchEvent> GetAll()
        {
            return _dbSet.ToList();
        }

        /// <summary>
        /// Recherche des événements selon un prédicat
        /// </summary>
        public IEnumerable<MatchEvent> Find(Expression<Func<MatchEvent, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }

        /// <summary>
        /// Ajoute un nouvel événement
        /// </summary>
        public void Add(MatchEvent entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Add(entity);
        }

        /// <summary>
        /// Ajoute plusieurs événements
        /// </summary>
        public void AddRange(IEnumerable<MatchEvent> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.AddRange(entities);
        }

        /// <summary>
        /// Met à jour un événement
        /// </summary>
        public void Update(MatchEvent entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
        }

        /// <summary>
        /// Supprime un événement
        /// </summary>
        public void Remove(MatchEvent entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Supprime plusieurs événements
        /// </summary>
        public void RemoveRange(IEnumerable<MatchEvent> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// Récupère tous les événements d'un match
        /// </summary>
        public async Task<IEnumerable<MatchEvent>> GetMatchEventsAsync(int matchId)
        {
            return await _dbSet
                .Where(e => e.MatchId == matchId)
                .OrderBy(e => e.Quarter)
                .ThenBy(e => e.GameTime)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les événements d'un quart-temps spécifique
        /// </summary>
        public async Task<IEnumerable<MatchEvent>> GetQuarterEventsAsync(int matchId, int quarter)
        {
            return await _dbSet
                .Where(e => e.MatchId == matchId && e.Quarter == quarter)
                .OrderBy(e => e.GameTime)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les fautes d'un joueur
        /// </summary>
        public async Task<IEnumerable<FoulEvent>> GetPlayerFoulsAsync(int matchId, int playerId)
        {
            return await _context.Set<FoulEvent>()
                .Where(f => f.MatchId == matchId && f.PlayerId == playerId)
                .OrderBy(f => f.Quarter)
                .ThenBy(f => f.GameTime)
                .ToListAsync();
        }

        /// <summary>
        /// Calcule les points marqués par un joueur
        /// </summary>
        public async Task<int> GetPlayerPointsAsync(int matchId, int playerId)
        {
            var baskets = await _context.Set<BasketScoredEvent>()
                .Where(b => b.MatchId == matchId && b.PlayerId == playerId)
                .ToListAsync();

            return baskets.Sum(b => b.Points);
        }
    }
}