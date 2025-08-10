using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BasketballLiveScore.Data;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;

namespace BasketballLiveScore.Repositories.Implementations
{
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        private readonly BasketballDbContext context;

        public TeamRepository(BasketballDbContext dbContext) : base(dbContext)
        {
            context = dbContext;
        }

        public override async Task<IEnumerable<Team>> GetAllAsync()
        {
            return await context.Teams.ToListAsync();
        }

        public override async Task<Team?> GetByIdAsync(int id)
        {
            return await context.Teams.FindAsync(id);
        }

        public override async Task AddAsync(Team team)
        {
            await context.Teams.AddAsync(team);
            await context.SaveChangesAsync();
        }

        public async Task<Team?> GetTeamWithPlayersAsync(int teamId)
        {
            return await context.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.Id == teamId);
        }

        public async Task<IEnumerable<Team>> GetActiveTeamsAsync()
        {
            return await context.Teams
                .Where(t => t.IsActive)  // Supposant un bool IsActive dans Team
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Team>> GetTeamsByCityAsync(string city)
        {
            return await context.Teams
                .Where(t => t.City.Contains(city))
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<bool> TeamExistsAsync(string name)
        {
            return await context.Teams
                .AnyAsync(t => t.Name.ToLower() == name.ToLower());
        }
    }
}
