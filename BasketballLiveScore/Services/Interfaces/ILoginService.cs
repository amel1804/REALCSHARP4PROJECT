using BasketballLiveScore.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace BasketballLiveScore.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service d'authentification
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// Authentifie un utilisateur avec son nom d'utilisateur et mot de passe
        /// </summary>
        /// <param name="username">Le nom d'utilisateur</param>
        /// <param name="password">Le mot de passe</param>
        /// <returns>L'utilisateur authentifié ou null si échec</returns>
        User? Login(string username, string password);

        /// <summary>
        /// Génère un token JWT pour l'utilisateur authentifié
        /// </summary>
        /// <param name="key">La clé secrète pour signer le token</param>
        /// <param name="claims">Les claims à inclure dans le token</param>
        /// <returns>Le token JWT généré</returns>
        string GenerateToken(string key, List<Claim> claims);

        /// <summary>
        /// Récupère tous les utilisateurs
        /// </summary>
        /// <returns>La liste de tous les utilisateurs</returns>
        List<User> GetAll();
    }
}