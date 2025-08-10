using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using BasketballLiveScore.Models;
using BasketballLiveScore.Services.Interfaces;
using BasketballLiveScore.Repositories.Interfaces;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service pour l'authentification des utilisateurs avec vérification de hash
    /// Génère les tokens JWT selon les bonnes pratiques
    /// </summary>
    public class LoginService : ILoginService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly ILogger<LoginService> _logger;

        public LoginService(
            IUnitOfWork unitOfWork,
            IPasswordHasherService passwordHasher,
            ILogger<LoginService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authentifie un utilisateur avec vérification du hash du mot de passe
        /// </summary>
        /// <param name="username">Nom d'utilisateur</param>
        /// <param name="password">Mot de passe en clair</param>
        /// <returns>L'utilisateur authentifié ou null si échec</returns>
        public User? Login(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Tentative de connexion avec des identifiants vides");
                    return null;
                }

                // Rechercher l'utilisateur par nom d'utilisateur
                var user = _unitOfWork.Users
                    .Find(u => u.Username == username && u.IsActive)
                    .FirstOrDefault();

                if (user == null)
                {
                    _logger.LogWarning("Tentative de connexion avec un utilisateur inexistant: {Username}", username);
                    return null;
                }

                // Vérifier le mot de passe
                bool isPasswordValid;

                // Si le mot de passe n'est pas hashé (migration), vérifier directement
                // TODO: Supprimer cette vérification après migration complète
                if (user.Password.Contains('.'))
                {
                    // Le mot de passe est hashé (contient le délimiteur salt.hash)
                    isPasswordValid = _passwordHasher.VerifyPassword(password, user.Password);
                }
                else
                {
                    // Ancien système - mot de passe en clair (à migrer)
                    isPasswordValid = user.Password == password;

                    // Migrer automatiquement vers le hash
                    if (isPasswordValid)
                    {
                        _logger.LogInformation("Migration du mot de passe pour l'utilisateur {Username}", username);
                        user.Password = _passwordHasher.HashPassword(password);
                        _unitOfWork.Users.Update(user);
                        _unitOfWork.Complete();
                    }
                }

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Échec de connexion - mot de passe incorrect pour: {Username}", username);
                    return null;
                }

                _logger.LogInformation("Connexion réussie pour l'utilisateur: {Username} avec le rôle {Role}",
                    username, user.Role);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la tentative de connexion pour {Username}", username);
                return null;
            }
        }

        /// <summary>
        /// Génère un token JWT pour l'utilisateur authentifié
        /// Basé sur les bonnes pratiques de sécurité JWT
        /// </summary>
        public string GenerateToken(string key, List<Claim> claims)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.Length < 32)
            {
                throw new ArgumentException("La clé JWT doit faire au moins 32 caractères pour la sécurité", nameof(key));
            }

            if (claims == null || !claims.Any())
            {
                throw new ArgumentNullException(nameof(claims));
            }

            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                // Ajouter des claims standards
                var allClaims = new List<Claim>(claims)
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat,
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                        ClaimValueTypes.Integer64)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(allClaims),
                    Expires = DateTime.UtcNow.AddHours(8), // Token valide 8 heures
                    SigningCredentials = credentials,
                    Issuer = "BasketballLiveScore",
                    Audience = "BasketballLiveScore.Users"
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                _logger.LogInformation("Token JWT généré avec succès pour l'utilisateur");

                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du token JWT");
                throw;
            }
        }

        /// <summary>
        /// Récupère tous les utilisateurs actifs
        /// </summary>
        public List<User> GetAll()
        {
            try
            {
                return _unitOfWork.Users
                    .Find(u => u.IsActive)
                    .OrderBy(u => u.Username)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les utilisateurs");
                return new List<User>();
            }
        }
    }
}