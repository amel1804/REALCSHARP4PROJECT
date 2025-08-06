using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BasketballLiveScore.Data;
using BasketballLiveScore.Repositories.Interfaces;

namespace BasketballLiveScore.Repositories.Implementations
{
    /// <summary>
    /// Repository g�n�rique impl�mentant les op�rations CRUD de base
    /// Bas� sur le pattern Repository vu dans les notes de cours
    /// </summary>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly BasketballDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(BasketballDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// R�cup�re une entit� par son identifiant
        /// </summary>
        public T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        /// <summary>
        /// R�cup�re une entit� par son identifiant de mani�re asynchrone
        /// Comme vu dans les notes sur la programmation asynchrone
        /// </summary>
        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// R�cup�re toutes les entit�s
        /// </summary>
        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        /// <summary>
        /// R�cup�re toutes les entit�s de mani�re asynchrone
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Recherche des entit�s selon un pr�dicat
        /// Utilise les expressions lambda comme vu dans les notes LINQ
        /// </summary>
        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }

        /// <summary>
        /// Recherche des entit�s selon un pr�dicat de mani�re asynchrone
        /// </summary>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Ajoute une nouvelle entit�
        /// </summary>
        public void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Add(entity);
        }

        /// <summary>
        /// Ajoute plusieurs entit�s en une seule op�ration
        /// M�thode utile pour les imports de donn�es en masse
        /// </summary>
        public void AddRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.AddRange(entities);
        }

        /// <summary>
        /// Met � jour une entit� existante
        /// </summary>
        public void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
        }

        /// <summary>
        /// Supprime une entit�
        /// </summary>
        public void Remove(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Supprime plusieurs entit�s en une seule op�ration
        /// </summary>
        public void RemoveRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.RemoveRange(entities);
        }
    }
}