using System.Collections.Generic;
using System.Threading.Tasks;
using BasketballLiveScore.Models;
using BasketballLiveScore.DTOs.User;
using BasketballLiveScore.DTOs;

namespace BasketballLiveScore.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetByIdAsync(int id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> AuthenticateAsync(string username, string password);
        Task<User> CreateAsync(UserCreateDto userDto);
        Task<User> UpdateAsync(int id, UserUpdateDto userDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
