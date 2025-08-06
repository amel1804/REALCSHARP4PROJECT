using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BasketballLiveScore.Models;
using BasketballLiveScore.Services.Interfaces;
using BasketballLiveScore.Repositories.Interfaces;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service pour l'authentification des utilisateurs
    /// Génère les tokens JWT comme vu dans les notes sur Authorization
    /// </summary>
    public class LoginService : ILoginService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LoginService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Authentifie un utilisateur
        /// </summary>
        public User Login(string name, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
                return null;

            return _unitOfWork.Users.GetByUsernameAndPassword(name, password);
        }

        /// <summary>
        /// Génère un token JWT pour l'utilisateur authentifié
        /// Basé sur l'exemple JWT dans les notes de cours
        /// </summary>
        public string GenerateToken(string key, List<Claim> claims)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (claims == null || !claims.Any())
                throw new ArgumentNullException(nameof(claims));

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Récupère tous les utilisateurs
        /// </summary>
        public List<User> GetAll()
        {
            return _unitOfWork.Users.GetAll().ToList();
        }
    }
}