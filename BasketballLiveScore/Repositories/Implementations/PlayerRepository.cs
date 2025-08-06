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
    /// Repository pour la gestion des joueurs
    /// </summary>
    public class PlayerRepository : Repository<Player>, IPlayerRepository
    {
        private readonly BasketballDbContext context;

        public PlayerRepository(BasketballDbContext dbContext) : base(dbContext)
        {
            context = dbContext;
        }

        /// <summary>
        /// R�cup�re les joueurs d'une �quipe
        /// </summary>
        public async Task<IEnumerable<Player>> GetPlayersByTeamAsync(int teamId)
        {
            return await context.Players
                .Where(p => p.TeamId == teamId)
                .OrderBy(p => p.JerseyNumber)
                .ToListAsync();
        }

        /// <summary>
        /// R�cup�re un joueur par son num�ro et son �quipe
        /// </summary>
        public async Task<Player> GetPlayerByNumberAndTeamAsync(int number, int teamId)
        {
            return await context.Players
                .FirstOrDefaultAsync(p => p.JerseyNumber == number && p.TeamId == teamId);
        }

        /// <summary>
        /// V�rifie si un num�ro est d�j� pris dans une �quipe
        /// </summary>
        public async Task<bool> IsNumberTakenInTeamAsync(int number, int teamId)
        {
            return await context.Players
                .AnyAsync(p => p.JerseyNumber == number && p.TeamId == teamId);
        }

        public override async Task<IEnumerable<Player>> GetAllAsync()
        {
            return await context.Players.ToListAsync();
        }

        public override async Task<Player?> GetByIdAsync(int id)
        {
            return await context.Players.FindAsync(id);
        }

        public override async Task AddAsync(Player player)
        {
            await context.Players.AddAsync(player);
            await context.SaveChangesAsync();
        }
    }
}