using System;
using System.Threading.Tasks;
using BasketballLiveScore.Data;
using BasketballLiveScore.Repositories.Interfaces;

namespace BasketballLiveScore.Repositories.Implementations
{
    /// <summary>
    /// Implémentation du pattern Unit of Work
    /// Centralise les transactions et l'accès aux repositories
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BasketballDbContext _context;
        private IUserRepository _users;
        private ITeamRepository _teams;
        private IPlayerRepository _players;
        private IMatchRepository _matches;
        private IGameActionRepository _gameActions;
        private IMatchEventRepository _matchEvents;
        private IMatchLineupRepository _matchLineups;

        public UnitOfWork(BasketballDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IUserRepository Users =>
            _users ??= new UserRepository(_context);

        public ITeamRepository Teams =>
            _teams ??= new TeamRepository(_context);

        public IPlayerRepository Players =>
            _players ??= new PlayerRepository(_context);

        public IMatchRepository Matches =>
            _matches ??= new MatchRepository(_context);

        public IGameActionRepository GameActions =>
            _gameActions ??= new GameActionRepository(_context);

        public IMatchEventRepository MatchEvents =>
            _matchEvents ??= new MatchEventRepository(_context);

        public IMatchLineupRepository MatchLineups =>
            _matchLineups ??= new MatchLineupRepository(_context);

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

    /// <summary>
    /// Repository générique de base
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly BasketballDbContext _context;

        public Repository(BasketballDbContext context)
        {
            _context = context;
        }

        public T GetById(int id)
        {
            return _context.Set<T>().Find(id);
        }

        public IEnumerable<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
        {
            return _context.Set<T>().Where(expression);
        }

        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities);
        }

        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }
    }

    /// <summary>
    /// Repository pour les utilisateurs
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(BasketballDbContext context) : base(context)
        {
        }

        public bool UserExists(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }

        public User GetByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public User GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }
    }

    /// <summary>
    /// Repository pour les équipes
    /// </summary>
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        public TeamRepository(BasketballDbContext context) : base(context)
        {
        }

        public Team GetTeamWithPlayers(int teamId)
        {
            return _context.Teams
                .Include(t => t.Players)
                .FirstOrDefault(t => t.Id == teamId);
        }

        public List<Team> GetActiveTeams()
        {
            return _context.Teams.ToList();
        }
    }

    /// <summary>
    /// Repository pour les joueurs
    /// </summary>
    public class PlayerRepository : Repository<Player>, IPlayerRepository
    {
        public PlayerRepository(BasketballDbContext context) : base(context)
        {
        }

        public List<Player> GetPlayersByTeam(int teamId)
        {
            return _context.Players.Where(p => p.TeamId == teamId).ToList();
        }

        public Player GetPlayerWithStats(int playerId)
        {
            return _context.Players
                .Include(p => p.Team)
                .FirstOrDefault(p => p.Id == playerId);
        }
    }

    /// <summary>
    /// Repository pour les actions de jeu
    /// </summary>
    public class GameActionRepository : Repository<GameAction>, IGameActionRepository
    {
        public GameActionRepository(BasketballDbContext context) : base(context)
        {
        }

        public List<GameAction> GetActionsByMatch(int matchId)
        {
            return _context.GameActions
                .Where(ga => ga.MatchId == matchId)
                .OrderBy(ga => ga.Quarter)
                .ThenBy(ga => ga.GameTime)
                .ToList();
        }

        public List<GameAction> GetActionsByQuarter(int matchId, int quarter)
        {
            return _context.GameActions
                .Where(ga => ga.MatchId == matchId && ga.Quarter == quarter)
                .OrderBy(ga => ga.GameTime)
                .ToList();
        }
    }

    /// <summary>
    /// Repository pour les événements de match
    /// </summary>
    public class MatchEventRepository : Repository<MatchEvent>, IMatchEventRepository
    {
        public MatchEventRepository(BasketballDbContext context) : base(context)
        {
        }

        public List<MatchEvent> GetEventsByMatch(int matchId)
        {
            return _context.MatchEvents
                .Where(me => me.MatchId == matchId)
                .OrderBy(me => me.CreatedAt)
                .ToList();
        }
    }

    /// <summary>
    /// Repository pour les compositions de match
    /// </summary>
    public class MatchLineupRepository : Repository<MatchLineup>, IMatchLineupRepository
    {
        public MatchLineupRepository(BasketballDbContext context) : base(context)
        {
        }

        public List<MatchLineup> GetLineupByMatch(int matchId)
        {
            return _context.MatchLineups
                .Include(ml => ml.Player)
                .Include(ml => ml.Team)
                .Where(ml => ml.MatchId == matchId)
                .ToList();
        }

        public List<MatchLineup> GetLineupByTeam(int matchId, int teamId)
        {
            return _context.MatchLineups
                .Include(ml => ml.Player)
                .Where(ml => ml.MatchId == matchId && ml.TeamId == teamId)
                .OrderBy(ml => ml.Position)
                .ToList();
        }

        public MatchLineup GetPlayerLineup(int matchId, int playerId)
        {
            return _context.MatchLineups
                .Include(ml => ml.Player)
                .FirstOrDefault(ml => ml.MatchId == matchId && ml.PlayerId == playerId);
        }
    }
}