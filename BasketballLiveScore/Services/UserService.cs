using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs;
using BasketballLiveScore.DTOs.User; // 📌 Ajout du bon using
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;

namespace BasketballLiveScore.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<User> GetByIdAsync(int id) =>
            await Task.FromResult(_unitOfWork.Users.GetById(id));

        public async Task<User> GetByUsernameAsync(string username) =>
            await Task.FromResult(_unitOfWork.Users.Find(u => u.Username == username).FirstOrDefault());

        public async Task<User> AuthenticateAsync(string username, string password) =>
            await Task.FromResult(_unitOfWork.Users.Find(u => u.Username == username && u.Password == password).FirstOrDefault());

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
                Password = userDto.Password, // ⚠️ à hasher dans la vraie vie
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
            if (user == null) return null;

            if (!string.IsNullOrEmpty(userDto.Name))
            {
                var parts = userDto.Name.Split(' ');
                user.FirstName = parts.FirstOrDefault() ?? user.FirstName;
                user.LastName = parts.Skip(1).FirstOrDefault() ?? user.LastName;
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
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<IEnumerable<User>> GetAllAsync() =>
            await Task.FromResult(_unitOfWork.Users.GetAll());

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = _unitOfWork.Users.GetById(userId);
            if (user == null || user.Password != currentPassword)
                return false;

            user.Password = newPassword;
            _unitOfWork.Users.Update(user);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}
