using BasketballLiveScore.Data;
using BasketballLiveScore.DTOs;
using BasketballLiveScore.Services.Interfaces;
using BasketballLiveScore.Models;

namespace BasketballLiveScore.Services
{
    public class GameActionService : IGameActionService
    {
        private readonly BasketballDbContext _context;

        public GameActionService(BasketballDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void RecordAction(GameActionDto actionDto)
        {
            if (actionDto == null)
                throw new ArgumentNullException(nameof(actionDto));

            var action = new GameAction
            {
                MatchId = actionDto.MatchId,
                PlayerName = actionDto.PlayerName ?? throw new ArgumentNullException(nameof(actionDto.PlayerName)),
                ActionType = actionDto.ActionType ?? throw new ArgumentNullException(nameof(actionDto.ActionType)),
                Points = actionDto.Points,
                FaultType = actionDto.FaultType,
                Quarter = actionDto.Quarter,
                Timestamp = actionDto.Timestamp
            };

            _context.GameActions.Add(action);
            _context.SaveChanges();
        }

        public List<GameAction> GetActionsForMatch(int matchId)
        {
            return _context.GameActions.Where(a => a.MatchId == matchId).ToList();
        }
    }
}
