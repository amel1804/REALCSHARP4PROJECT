using BasketballLiveScore.Data;
using BasketballLiveScore.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using BasketballLiveScore.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace BasketballLiveScore.Repositories.Implementations
{
    public class MatchRepository : Repository<Match>, IMatchRepository
    {
        private readonly BasketballDbContext _context;

        public MatchRepository(BasketballDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Match>> GetMatchesWithTeamsAsync()
        {
            return await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .ToListAsync();
        }

        public async Task<List<Match>> GetMatchesByStatusAsync(MatchStatus status)
        {
            return await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.Status == status)
                .ToListAsync();
        }

        public async Task<List<Match>> GetMatchesByTeamAsync(int teamId)
        {
            return await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                .ToListAsync();
        }

        public async Task<Match> GetMatchWithDetailsAsync(int matchId)
        {
            return await _context.Matches
                .Include(m => m.HomeTeam).ThenInclude(t => t.Players)
                .Include(m => m.AwayTeam).ThenInclude(t => t.Players)
                .Include(m => m.Events)
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }
    }
}
