using System;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Repositories.Interfaces
{
    /// <summary>
    /// Interface pour le repository des utilisateurs
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Vérifie si un utilisateur existe
        /// </summary>
        bool UserExists(string username);

        /// <summary>
        /// Récupère un utilisateur par nom et mot de passe
        /// </summary>
        User GetByUsernameAndPassword(string username, string password);
    }
}