using System;
using Microsoft.Extensions.Logging;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service pour l'enregistrement des utilisateurs avec hashage sécurisé
    /// </summary>
    public class RegisterService : IRegisterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly ILogger<RegisterService> _logger;

        public RegisterService(
            IUnitOfWork unitOfWork,
            IPasswordHasherService passwordHasher,
            ILogger<RegisterService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur avec mot de passe hashé
        /// </summary>
        /// <param name="username">Nom d'utilisateur</param>
        /// <param name="password">Mot de passe en clair (sera hashé)</param>
        /// <param name="role">Rôle de l'utilisateur</param>
        /// <returns>"OK" si succès, message d'erreur sinon</returns>
        public string Register(string username, string password, string role)
        {
            try
            {
                // Validation des paramètres
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Tentative d'enregistrement avec nom d'utilisateur vide");
                    return "Le nom d'utilisateur est obligatoire";
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Tentative d'enregistrement avec mot de passe vide");
                    return "Le mot de passe est obligatoire";
                }

                if (password.Length < 6)
                {
                    _logger.LogWarning("Tentative d'enregistrement avec mot de passe trop court");
                    return "Le mot de passe doit contenir au moins 6 caractères";
                }

                if (string.IsNullOrWhiteSpace(role))
                {
                    _logger.LogWarning("Tentative d'enregistrement sans rôle");
                    return "Le rôle est obligatoire";
                }

                // Valider le rôle
                var validRoles = new[] { UserRoles.ADMINISTRATOR, UserRoles.ENCODER, UserRoles.VIEWER };
                if (!Array.Exists(validRoles, r => r.Equals(role, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning("Tentative d'enregistrement avec rôle invalide: {Role}", role);
                    return $"Rôle invalide. Rôles valides: {string.Join(", ", validRoles)}";
                }

                // Vérification si l'utilisateur existe déjà
                if (_unitOfWork.Users.UserExists(username))
                {
                    _logger.LogWarning("Tentative d'enregistrement d'un utilisateur existant: {Username}", username);
                    return "Ce nom d'utilisateur est déjà utilisé";
                }

                // Hasher le mot de passe
                string hashedPassword = _passwordHasher.HashPassword(password);

                // Extraction du prénom et nom depuis le username (temporaire)
                var nameParts = username.Split('_', ' ', '.', '-');
                string firstName = nameParts.Length > 0 ? nameParts[0] : username;
                string lastName = nameParts.Length > 1 ? string.Join(" ", nameParts[1..]) : "User";

                // Création du nouvel utilisateur
                var user = new User
                {
                    Username = username,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = $"{username.ToLower()}@basketballlive.com",
                    Password = hashedPassword, // Mot de passe hashé
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Users.Add(user);
                _unitOfWork.Complete();

                _logger.LogInformation("Nouvel utilisateur enregistré avec succès: {Username} avec le rôle {Role}",
                    username, role);

                return "OK";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de l'utilisateur {Username}", username);
                return "Une erreur est survenue lors de l'enregistrement";
            }
        }
    }
}