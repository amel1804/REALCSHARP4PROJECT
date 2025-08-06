using BasketballLiveScore.Models;

namespace BasketballLiveScore.Services.Interfaces
{
    public interface IUpdateService
    {
        string Update(int id, string name, string role);
    }
}
