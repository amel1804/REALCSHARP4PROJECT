using System.Threading.Tasks;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Repositories.Interfaces
{
    public interface IMatchLineupRepository : IRepository<MatchLineup>
    {
        Task<MatchLineup?> GetPlayerLineup(int matchId, int playerId);
    }
}