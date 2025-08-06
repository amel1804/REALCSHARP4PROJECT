using BasketballLiveScore.Models;
using System.Security.Claims;

namespace BasketballLiveScore.Services.Interfaces
{
    public interface ILoginService
    {
        User Login(string name, string password);
        string GenerateToken(string key, List<Claim> claims);
        List<User> GetAll();
    }
}
