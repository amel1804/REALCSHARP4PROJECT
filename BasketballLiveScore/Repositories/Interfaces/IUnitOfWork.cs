using System;
using System.Threading.Tasks;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface Unit of Work pour gérer les transactions
    /// Pattern vu dans les cours sur Entity Framework
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ITeamRepository Teams { get; }
        IPlayerRepository Players { get; }
        IMatchRepository Matches { get; }
        IMatchEventRepository MatchEvents { get; }

        /// <summary>
        /// Sauvegarde tous les changements en base
        /// </summary>
        int Complete();

        /// <summary>
        /// Sauvegarde asynchrone
        /// </summary>
        Task<int> CompleteAsync();
    }
}