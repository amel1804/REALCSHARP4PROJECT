using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using BasketballLiveScore.Data;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;

namespace BasketballLiveScore.Repositories.Implementations
{
    /// <summary>
    /// Repository pour la gestion des utilisateurs
    /// Implémente les opérations spécifiques aux utilisateurs
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly BasketballDbContext _context;
        private readonly DbSet<User> _dbSet;

        public UserRepository(BasketballDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<User>();
        }

        /// <summary>
        /// Récupère un utilisateur par son identifiant
        /// </summary>
        public User GetById(int id)
        {
            return _dbSet.Find(id);
        }

        /// <summary>
        /// Récupère tous les utilisateurs
        /// </summary>
        public IEnumerable<User> GetAll()
        {
            return _dbSet.ToList();
        }

        /// <summary>
        /// Recherche des utilisateurs selon un prédicat
        /// </summary>
        public IEnumerable<User> Find(Expression<Func<User, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }

        /// <summary>
        /// Ajoute un nouvel utilisateur
        /// </summary>
        public void Add(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Add(entity);
        }

        /// <summary>
        /// Ajoute plusieurs utilisateurs
        /// Utile pour l'import en masse
        /// </summary>
        public void AddRange(IEnumerable<User> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.AddRange(entities);
        }

        /// <summary>
        /// Met à jour un utilisateur
        /// </summary>
        public void Update(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
        }

        /// <summary>
        /// Supprime un utilisateur
        /// </summary>
        public void Remove(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Supprime plusieurs utilisateurs
        /// </summary>
        public void RemoveRange(IEnumerable<User> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.RemoveRange(entities);
        }

        // Remplacez ces méthodes dans votre UserRepository.cs :

        /// <summary>
        /// Récupère un utilisateur par son nom et mot de passe
        /// Utilisé pour l'authentification
        /// </summary>
        public User? GetByUsernameAndPassword(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            return _dbSet.FirstOrDefault(u =>
                u.Username == username &&  // Utiliser Username au lieu de Name
                u.Password == password &&
                u.IsActive);
        }

        /// <summary>
        /// Vérifie si un utilisateur existe par son nom
        /// </summary>
        public bool UserExists(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            return _dbSet.Any(u => u.Username == username);  // Utiliser Username au lieu de Name
        }

        /// <summary>
        /// Récupère un utilisateur par son nom
        /// </summary>
        public User? GetByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            return _dbSet.FirstOrDefault(u => u.Username == username);  // Utiliser Username au lieu de Name
        }
        /// <summary>
        /// Récupère les utilisateurs actifs
        /// </summary>
        public IEnumerable<User> GetActiveUsers()
        {
            return _dbSet.Where(u => u.IsActive).ToList();
        }
    }
}