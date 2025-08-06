using System;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service pour l'enregistrement des utilisateurs
    /// Utilise le pattern Repository comme dans VideoGameManager
    /// </summary>
    public class RegisterService : IRegisterService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegisterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur
        /// Vérifie que l'utilisateur n'existe pas déjà
        /// </summary>
        public string Register(string name, string password, string role)
        {
            // Validation des paramètres
            if (string.IsNullOrEmpty(name))
                return "Name is required";

            if (string.IsNullOrEmpty(password))
                return "Password is required";

            if (string.IsNullOrEmpty(role))
                return "Role is required";

            // Vérification si l'utilisateur existe déjà
            if (_unitOfWork.Users.UserExists(name))
                return "User already exists";

            // Création du nouvel utilisateur
            var user = new User
            {
                Name = name,
                Password = password,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            try
            {
                _unitOfWork.Users.Add(user);
                _unitOfWork.Complete();
                return "OK";
            }
            catch (Exception ex)
            {
                // Log l'erreur en production
                return $"Registration failed: {ex.Message}";
            }
        }
    }
}