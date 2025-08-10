using System.Threading.Tasks;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        bool UserExists(string username);
        Task<User?> GetByEmailAsync(string email);
        User? GetByUsernameAndPassword(string username, string password);
        User? GetByUsername(string username);
        Task<IEnumerable<User>> GetActiveUsersAsync();
    }
}
