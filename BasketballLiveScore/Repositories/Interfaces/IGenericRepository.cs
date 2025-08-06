using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface générique pour le pattern Repository
    /// Basé sur le pattern vu dans les cours avec VideoGameManager
    /// Note: J'utilise IGenericRepository au lieu de IRepository pour éviter toute confusion
    /// </summary>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        // Méthodes synchrones
        TEntity GetById(int id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);

        // Méthodes asynchrones comme dans les notes async/await
        Task<TEntity> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    }
}