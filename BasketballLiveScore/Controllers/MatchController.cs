using Microsoft.AspNetCore.Mvc;
using BasketballLiveScore.Services.Interfaces;
using BasketballLiveScore.DTOs.Match; // <-- correction ici

namespace BasketballLiveScore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly IMatchService _matchService;

        public MatchController(IMatchService matchService)
        {
            _matchService = matchService;
        }

        [HttpPost]
        public IActionResult CreateMatch([FromBody] MatchDto matchDto)
        {
            _matchService.CreateMatch(matchDto);
            return Ok("Match created successfully");
        }

        [HttpGet]
        public IActionResult GetAllMatches()
        {
            return Ok(_matchService.GetAllMatches());
        }

        [HttpGet("{id}")]
        public IActionResult GetMatchById(int id)
        {
            var match = _matchService.GetMatchById(id);
            if (match == null) return NotFound();
            return Ok(match);
        }
    }
}
