using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface g�n�rique pour le pattern Repository
    /// Bas� sur le pattern vu dans les cours avec VideoGameManager
    /// Note: J'utilise IGenericRepository au lieu de IRepository pour �viter toute confusion
    /// </summary>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        // M�thodes synchrones
        TEntity GetById(int id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);

        // M�thodes asynchrones comme dans les notes async/await
        Task<TEntity> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    }
}