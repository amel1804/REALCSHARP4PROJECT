using System.Collections.Generic;
using System.Threading.Tasks;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Repositories.Interfaces
{
    public interface ITeamRepository : IRepository<Team>
    {
        Task<Team?> GetTeamWithPlayersAsync(int teamId);
        Task<IEnumerable<Team>> GetActiveTeamsAsync();
        Task<IEnumerable<Team>> GetTeamsByCityAsync(string city);
        Task<bool> TeamExistsAsync(string teamName);
    }
}
