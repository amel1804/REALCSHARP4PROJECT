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

        // Ajouter cette méthode dans votre MatchController existant

        /// <summary>
        /// Met à jour un match existant
        /// Utilisé notamment pour changer le statut du match
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult UpdateMatch(int id, [FromBody] MatchDto matchDto)
        {
            if (matchDto == null)
            {
                return BadRequest("Invalid match data");
            }

            if (id != matchDto.Id)
            {
                return BadRequest("Match ID mismatch");
            }

            try
            {
                // Récupération du match existant
                var existingMatch = _matchService.GetMatchById(id);
                if (existingMatch == null)
                {
                    return NotFound($"Match with ID {id} not found");
                }

                // Mise à jour via le service
                _matchService.UpdateMatch(matchDto);

                return Ok("Match updated successfully");
            }
            catch (Exception ex)
            {
                // Log de l'erreur en production
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
