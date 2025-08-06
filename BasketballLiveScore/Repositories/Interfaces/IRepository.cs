using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface générique pour le pattern Repository
    /// </summary>
    public interface IRepository<TEntity> where TEntity : class
    {
        // Méthode pour récupérer une entité par son ID
        TEntity GetById(int id);

        // Méthode pour récupérer toutes les entités
        IEnumerable<TEntity> GetAll();

        // Méthode pour trouver des entités selon un prédicat
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        // Méthode pour ajouter une entité
        void Add(TEntity entity);

        // Méthode pour ajouter plusieurs entités
        void AddRange(IEnumerable<TEntity> entities);

        // Méthode pour supprimer une entité
        void Remove(TEntity entity);

        // Méthode pour supprimer plusieurs entités
        void RemoveRange(IEnumerable<TEntity> entities);

        // Méthode pour mettre à jour une entité
        void Update(TEntity entity);
    }
}
