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
    /// Repository générique implémentant les opérations CRUD de base
    /// Basé sur le pattern Repository vu dans les notes de cours
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
        /// Récupère une entité par son identifiant
        /// </summary>
        public T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        /// <summary>
        /// Récupère une entité par son identifiant de manière asynchrone
        /// Comme vu dans les notes sur la programmation asynchrone
        /// </summary>
        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Récupère toutes les entités
        /// </summary>
        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        /// <summary>
        /// Récupère toutes les entités de manière asynchrone
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Recherche des entités selon un prédicat
        /// Utilise les expressions lambda comme vu dans les notes LINQ
        /// </summary>
        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }

        /// <summary>
        /// Recherche des entités selon un prédicat de manière asynchrone
        /// </summary>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Ajoute une nouvelle entité
        /// </summary>
        public void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Add(entity);
        }

        /// <summary>
        /// Ajoute plusieurs entités en une seule opération
        /// Méthode utile pour les imports de données en masse
        /// </summary>
        public void AddRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.AddRange(entities);
        }

        /// <summary>
        /// Met à jour une entité existante
        /// </summary>
        public void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
        }

        /// <summary>
        /// Supprime une entité
        /// </summary>
        public void Remove(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Supprime plusieurs entités en une seule opération
        /// </summary>
        public void RemoveRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.RemoveRange(entities);
        }
    }
}