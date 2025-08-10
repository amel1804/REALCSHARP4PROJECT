using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface générique pour les repositories
    /// </summary>
    public interface IRepository<T> where T : class
    {
        T GetById(int id);
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void Update(T entity);
    }

    /// <summary>
    /// Interface pour le repository des utilisateurs
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        bool UserExists(string username);
        User GetByUsername(string username);
        User GetByEmail(string email);
    }

    /// <summary>
    /// Interface pour le repository des équipes
    /// </summary>
    public interface ITeamRepository : IRepository<Team>
    {
        Team GetTeamWithPlayers(int teamId);
        List<Team> GetActiveTeams();
    }

    /// <summary>
    /// Interface pour le repository des joueurs
    /// </summary>
    public interface IPlayerRepository : IRepository<Player>
    {
        List<Player> GetPlayersByTeam(int teamId);
        Player GetPlayerWithStats(int playerId);
    }

    /// <summary>
    /// Interface pour le repository des actions de jeu
    /// </summary>
    public interface IGameActionRepository : IRepository<GameAction>
    {
        List<GameAction> GetActionsByMatch(int matchId);
        List<GameAction> GetActionsByQuarter(int matchId, int quarter);
    }

    /// <summary>
    /// Interface pour le repository des événements de match
    /// </summary>
    public interface IMatchEventRepository : IRepository<MatchEvent>
    {
        List<MatchEvent> GetEventsByMatch(int matchId);
    }

    /// <summary>
    /// Interface pour le repository des compositions de match
    /// </summary>
    public interface IMatchLineupRepository : IRepository<MatchLineup>
    {
        List<MatchLineup> GetLineupByMatch(int matchId);
        List<MatchLineup> GetLineupByTeam(int matchId, int teamId);
        MatchLineup GetPlayerLineup(int matchId, int playerId);
    }

    /// <summary>
    /// Interface Unit of Work pour gérer les transactions
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ITeamRepository Teams { get; }
        IPlayerRepository Players { get; }
        IMatchRepository Matches { get; }
        IGameActionRepository GameActions { get; }
        IMatchEventRepository MatchEvents { get; }
        IMatchLineupRepository MatchLineups { get; }

        int Complete();
        Task<int> CompleteAsync();
    }
}