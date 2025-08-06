using BasketballLiveScore.DTOs;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BasketballLiveScore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameActionController : ControllerBase
    {
        private readonly IGameActionService _gameActionService;

        public GameActionController(IGameActionService gameActionService)
        {
            _gameActionService = gameActionService;
        }

        [HttpPost]
        public IActionResult RecordAction([FromBody] GameActionDto actionDto)
        {
            _gameActionService.RecordAction(actionDto);
            return Ok("Action recorded successfully");
        }

        [HttpGet("{matchId}")]
        public IActionResult GetActionsForMatch(int matchId)
        {
            var actions = _gameActionService.GetActionsForMatch(matchId);
            return Ok(actions);
        }
    }
}
