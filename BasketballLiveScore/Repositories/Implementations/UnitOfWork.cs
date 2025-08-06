using System;
using System.Threading.Tasks;
using BasketballLiveScore.Data;
using BasketballLiveScore.Repositories.Interfaces;

namespace BasketballLiveScore.Repositories.Implementations
{
    /// <summary>
    /// Implémentation du pattern Unit of Work
    /// Centralise la gestion des repositories et des transactions
    /// Comme vu dans les notes sur Entity Framework
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly BasketballDbContext _context;
        private bool _disposed = false;

        // Repositories - Lazy loading comme vu dans les notes
        private IUserRepository _users;
        private ITeamRepository _teams;
        private IPlayerRepository _players;
        private IMatchRepository _matches;
        private IMatchEventRepository _matchEvents;

        public UnitOfWork(BasketballDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Repository des utilisateurs
        /// Utilise le pattern Lazy Loading pour optimiser les performances
        /// </summary>
        public IUserRepository Users => _users ??= new UserRepository(_context);

        /// <summary>
        /// Repository des équipes
        /// </summary>
        public ITeamRepository Teams => _teams ??= new TeamRepository(_context);

        /// <summary>
        /// Repository des joueurs
        /// </summary>
        public IPlayerRepository Players => _players ??= new PlayerRepository(_context);

        /// <summary>
        /// Repository des matchs
        /// </summary>
        public IMatchRepository Matches => _matches ??= new MatchRepository(_context);

        /// <summary>
        /// Repository des événements de match
        /// </summary>
        public IMatchEventRepository MatchEvents => _matchEvents ??= new MatchEventRepository(_context);

        /// <summary>
        /// Sauvegarde tous les changements dans la base de données
        /// Utilise le pattern transactionnel d'Entity Framework
        /// </summary>
        public int Complete()
        {
            try
            {
                return _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // En production, logger l'exception
                throw new InvalidOperationException("Erreur lors de la sauvegarde des changements", ex);
            }
        }

        /// <summary>
        /// Sauvegarde tous les changements de manière asynchrone
        /// Comme vu dans les notes sur la programmation asynchrone
        /// </summary>
        public async Task<int> CompleteAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // En production, logger l'exception
                throw new InvalidOperationException("Erreur lors de la sauvegarde asynchrone des changements", ex);
            }
        }

        /// <summary>
        /// Libère les ressources du contexte
        /// Pattern Dispose vu dans les notes de cours
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}