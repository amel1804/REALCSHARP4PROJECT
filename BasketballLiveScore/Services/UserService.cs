
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs.User;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;
using BasketballLiveScore.DTOs;


namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service de gestion des utilisateurs.
    /// Utilise IUnitOfWork et IPasswordHasherService pour le hachage des mots de passe.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasherService _passwordHasher;

        public UserService(IUnitOfWork unitOfWork, IPasswordHasherService passwordHasher)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            var user = _unitOfWork.Users.GetById(id);
            return await Task.FromResult(user);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var user = _unitOfWork.Users.Find(u => u.Username == username).FirstOrDefault();
            return await Task.FromResult(user);
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = _unitOfWork.Users.Find(u => u.Username == username).FirstOrDefault();
            if (user == null) return null;

            // Vérifier via le service de hachage
            var verified = _passwordHasher.VerifyPassword(password, user.Password);
            return await Task.FromResult(verified ? user : null);
        }

        public async Task<User> CreateAsync(UserCreateDto userDto)
        {
            if (userDto == null) throw new ArgumentNullException(nameof(userDto));

            // Vérifications basiques (uniqueness, email, etc.)
            var existing = _unitOfWork.Users.Find(u => u.Username.ToLower() == userDto.Username.ToLower()).FirstOrDefault();
            if (existing != null)
                throw new InvalidOperationException($"Le nom d'utilisateur '{userDto.Username}' est déjà utilisé.");

            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Username = userDto.Username,
                Password = _passwordHasher.HashPassword(userDto.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _unitOfWork.Users.Add(user);
            await _unitOfWork.CompleteAsync();

            return user;
        }

        public async Task<User?> UpdateAsync(int id, UserUpdateDto userDto)
        {
            var user = _unitOfWork.Users.GetById(id);
            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(userDto.Name))
            {
                var parts = userDto.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                user.FirstName = parts.FirstOrDefault() ?? user.FirstName;
                user.LastName = parts.Skip(1).FirstOrDefault() ?? user.LastName;
            }

            if (!string.IsNullOrWhiteSpace(userDto.Email))
            {
                user.Email = userDto.Email;
            }

            if (userDto.IsActive.HasValue)
            {
                user.IsActive = userDto.IsActive.Value;
            }

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = _unitOfWork.Users.GetById(id);
            if (user == null) return false;

            _unitOfWork.Users.Remove(user);
            var result = await _unitOfWork.CompleteAsync();
            return result > 0;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = _unitOfWork.Users.GetAll() ?? Enumerable.Empty<User>();
            return await Task.FromResult(users);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = _unitOfWork.Users.GetById(userId);
            if (user == null) return false;

            // Vérifier l'ancien mot de passe avant changement
            if (!_passwordHasher.VerifyPassword(currentPassword, user.Password))
                return false;

            user.Password = _passwordHasher.HashPassword(newPassword);
            _unitOfWork.Users.Update(user);
            var result = await _unitOfWork.CompleteAsync();
            return result > 0;
        }
    }
}
