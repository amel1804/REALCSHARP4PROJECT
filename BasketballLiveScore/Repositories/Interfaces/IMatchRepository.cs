using System.Collections.Generic;
using System.Threading.Tasks;
using BasketballLiveScore.Models;


namespace BasketballLiveScore.Repositories.Interfaces
{
    public interface IMatchRepository : IRepository<Match>
    {
        Task<List<Match>> GetMatchesWithTeamsAsync();

        // Utilisation de l'enum MatchStatus du namespace BasketballLiveScore.Models
        Task<List<Match>> GetMatchesByStatusAsync(MatchStatus status);

        Task<List<Match>> GetMatchesByTeamAsync(int teamId);

        Task<Match> GetMatchWithDetailsAsync(int matchId);
    }
}
