using BasketballLiveScore.Models;

namespace BasketballLiveScore.Services.Interfaces
{
    public interface IRegisterService
    {
        string Register(string name, string password, string role);
    }
}
