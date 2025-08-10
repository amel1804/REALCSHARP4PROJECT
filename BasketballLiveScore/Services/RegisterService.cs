using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service pour l'enregistrement des utilisateurs avec hashage sécurisé
    /// </summary>
    public class RegisterService : IRegisterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RegisterService> _logger;

        public RegisterService(
            IUnitOfWork unitOfWork,
            ILogger<RegisterService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur - Version simplifiée pour compatibilité
        /// </summary>
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
                    role = "Viewer"; // Rôle par défaut
                }

                // Valider le rôle
                var validRoles = new[] { "Administrator", "Encoder", "Viewer" };
                if (!validRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
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
                string hashedPassword = HashPassword(password);

                // Extraction du prénom et nom depuis le username (temporaire)
                var nameParts = username.Split(new[] { '_', ' ', '.', '-' }, StringSplitOptions.RemoveEmptyEntries);
                string firstName = nameParts.Length > 0 ? nameParts[0] : username;
                string lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "User";

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

        /// <summary>
        /// Enregistre un nouvel utilisateur avec toutes les données
        /// </summary>
        public string RegisterComplete(string firstName, string lastName, string username,
            string email, string password, string role)
        {
            try
            {
                // Validation complète
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    return "Le prénom et le nom sont obligatoires";
                }

                if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
                {
                    return "Le nom d'utilisateur doit contenir au moins 3 caractères";
                }

                if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
                {
                    return "L'adresse email n'est pas valide";
                }

                if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                {
                    return "Le mot de passe doit contenir au moins 6 caractères";
                }

                // Vérification de l'unicité
                if (_unitOfWork.Users.UserExists(username))
                {
                    return "Ce nom d'utilisateur est déjà utilisé";
                }

                var existingEmail = _unitOfWork.Users.GetByEmail(email);
                if (existingEmail != null)
                {
                    return "Cette adresse email est déjà utilisée";
                }

                // Validation du rôle
                var validRoles = new[] { "Administrator", "Encoder", "Viewer" };
                if (!validRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                {
                    role = "Viewer"; // Rôle par défaut
                }

                // Hashage du mot de passe
                string hashedPassword = HashPassword(password);

                // Création de l'utilisateur
                var user = new User
                {
                    FirstName = firstName.Trim(),
                    LastName = lastName.Trim(),
                    Username = username.Trim(),
                    Email = email.Trim().ToLower(),
                    Password = hashedPassword,
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Users.Add(user);
                _unitOfWork.Complete();

                _logger.LogInformation("Utilisateur complet enregistré: {Username} ({Email})", username, email);

                return "OK";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement complet");
                return "Une erreur est survenue lors de l'enregistrement";
            }
        }

        /// <summary>
        /// Hash un mot de passe avec PBKDF2-SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            // Génération d'un salt aléatoire
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hashage du mot de passe
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            // Retour au format salt.hash en base64
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Valide le format d'une adresse email
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}