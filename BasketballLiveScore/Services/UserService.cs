using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs;
using BasketballLiveScore.DTOs.User;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service pour la gestion des utilisateurs
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await Task.FromResult(_unitOfWork.Users.GetById(id));
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            var user = _unitOfWork.Users.Find(u => u.Username == username).FirstOrDefault();
            return await Task.FromResult(user);
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            // Récupérer l'utilisateur
            var user = _unitOfWork.Users.Find(u => u.Username == username).FirstOrDefault();
            if (user == null) return null;

            // Vérifier le mot de passe hashé
            if (VerifyPassword(password, user.Password))
            {
                return await Task.FromResult(user);
            }

            return null;
        }

        public async Task<User> CreateAsync(UserCreateDto userDto)
        {
            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto));

            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Username = userDto.Username,
                Password = HashPassword(userDto.Password), // HASHAGE DU MOT DE PASSE
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _unitOfWork.Users.Add(user);
            await _unitOfWork.CompleteAsync();

            return user;
        }

        public async Task<User> UpdateAsync(int id, UserUpdateDto userDto)
        {
            var user = _unitOfWork.Users.GetById(id);
            if (user == null)
                return null;

            if (!string.IsNullOrEmpty(userDto.Name))
            {
                var nameParts = userDto.Name.Split(' ');
                user.FirstName = nameParts.FirstOrDefault() ?? user.FirstName;
                user.LastName = nameParts.Skip(1).FirstOrDefault() ?? user.LastName;
            }

            if (!string.IsNullOrEmpty(userDto.Role))
            {
                user.Role = userDto.Role;
            }

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = _unitOfWork.Users.GetById(id);
            if (user == null)
                return false;

            _unitOfWork.Users.Remove(user);
            var result = await _unitOfWork.CompleteAsync();

            return result > 0;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await Task.FromResult(_unitOfWork.Users.GetAll());
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = _unitOfWork.Users.GetById(userId);
            if (user == null)
                return false;

            // Vérifier l'ancien mot de passe hashé
            if (!VerifyPassword(currentPassword, user.Password))
                return false;

            // Hasher le nouveau mot de passe
            user.Password = HashPassword(newPassword);

            _unitOfWork.Users.Update(user);
            var result = await _unitOfWork.CompleteAsync();

            return result > 0;
        }

        /// <summary>
        /// Hash un mot de passe avec PBKDF2
        /// </summary>
        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        /// <summary>
        /// Vérifie un mot de passe contre son hash
        /// </summary>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || !hashedPassword.Contains("."))
            {
                // Pour migration : si pas de point, c'est un ancien mot de passe non hashé
                return password == hashedPassword;
            }

            var parts = hashedPassword.Split('.');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            string computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hash == computedHash;
        }
    }
}