using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BasketballLiveScore.Data;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;

namespace BasketballLiveScore.Repositories.Implementations
{
    public class MatchLineupRepository : Repository<MatchLineup>, IMatchLineupRepository
    {
        public MatchLineupRepository(BasketballDbContext context) : base(context)
        {
        }

        public async Task<MatchLineup?> GetPlayerLineup(int matchId, int playerId)
        {
            return await Context.Set<MatchLineup>()
                .Include(ml => ml.Player)
                .FirstOrDefaultAsync(ml => ml.MatchId == matchId && ml.PlayerId == playerId);
        }
    }
}