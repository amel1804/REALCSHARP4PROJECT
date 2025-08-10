using System.Collections.Generic;
using System.Threading.Tasks;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Repositories.Interfaces
{
    public interface IPlayerRepository : IRepository<Player>
    {
        Task<IEnumerable<Player>> GetPlayersByTeamAsync(int teamId);
        Task<Player?> GetPlayerWithStats(int playerId);
        Task<Player> GetPlayerByNumberAndTeamAsync(int number, int teamId);
        Task<bool> IsNumberTakenInTeamAsync(int number, int teamId);
    }
}
