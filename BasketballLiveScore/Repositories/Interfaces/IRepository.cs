using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface g�n�rique pour le pattern Repository
    /// </summary>
    public interface IRepository<TEntity> where TEntity : class
    {
        // M�thode pour r�cup�rer une entit� par son ID
        TEntity GetById(int id);

        // M�thode pour r�cup�rer toutes les entit�s
        IEnumerable<TEntity> GetAll();

        // M�thode pour trouver des entit�s selon un pr�dicat
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        // M�thode pour ajouter une entit�
        void Add(TEntity entity);

        // M�thode pour ajouter plusieurs entit�s
        void AddRange(IEnumerable<TEntity> entities);

        // M�thode pour supprimer une entit�
        void Remove(TEntity entity);

        // M�thode pour supprimer plusieurs entit�s
        void RemoveRange(IEnumerable<TEntity> entities);

        // M�thode pour mettre � jour une entit�
        void Update(TEntity entity);
    }
}