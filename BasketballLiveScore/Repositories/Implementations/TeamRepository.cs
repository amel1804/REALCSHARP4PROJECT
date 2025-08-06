using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BasketballLiveScore.Data;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;

namespace BasketballLiveScore.Repositories.Implementations
{
    /// <summary>
    /// Repository pour la gestion des �quipes
    /// </summary>
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        private readonly BasketballDbContext context;

        public TeamRepository(BasketballDbContext dbContext) : base(dbContext)
        {
            context = dbContext;
        }

        /// <summary>
        /// R�cup�re une �quipe avec ses joueurs
        /// </summary>
        public async Task<Team> GetTeamWithPlayersAsync(int teamId)
        {
            return await context.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.Id == teamId);
        }

        /// <summary>
        /// R�cup�re les �quipes par ville
        /// </summary>
        public async Task<IEnumerable<Team>> GetTeamsByCityAsync(string city)
        {
            return await context.Teams
                .Where(t => t.City.Contains(city))
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        /// <summary>
        /// V�rifie si une �quipe existe d�j�
        /// </summary>
        public async Task<bool> TeamExistsAsync(string name)
        {
            return await context.Teams
                .AnyAsync(t => t.Name.ToLower() == name.ToLower());
        }
    }
}