using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BasketballLiveScore.DTOs.Player;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BasketballLiveScore.Controllers
{
    /// <summary>
    /// Contr�leur pour la gestion des joueurs
    /// G�re toutes les op�rations CRUD sur les joueurs
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // N�cessite une authentification
    public class PlayerController : ControllerBase
    {
        // Constantes pour �viter les valeurs magiques
        private const int MIN_JERSEY_NUMBER = 0;
        private const int MAX_JERSEY_NUMBER = 99;
        private const int STARTING_FIVE_COUNT = 5;

        private readonly IPlayerService _playerService;
        private readonly ILogger<PlayerController> _logger;

        /// <summary>
        /// Constructeur avec injection de d�pendances
        /// </summary>
        public PlayerController(IPlayerService playerService, ILogger<PlayerController> logger)
        {
            _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// R�cup�re tous les joueurs
        /// GET: api/Player
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PlayerDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<PlayerDto>>> GetAllPlayers()
        {
            try
            {
                _logger.LogInformation("R�cup�ration de tous les joueurs");
                var players = await _playerService.GetAllPlayersAsync();
                return Ok(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration des joueurs");
                return StatusCode(500, "Une erreur est survenue lors de la r�cup�ration des joueurs");
            }
        }

        /// <summary>
        /// R�cup�re un joueur par son identifiant
        /// GET: api/Player/5
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PlayerDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<PlayerDto>> GetPlayerById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("L'identifiant du joueur doit �tre sup�rieur � z�ro");
                }

                var player = await _playerService.GetPlayerByIdAsync(id);

                if (player == null)
                {
                    _logger.LogWarning("Joueur avec l'ID {PlayerId} non trouv�", id);
                    return NotFound($"Joueur avec l'ID {id} non trouv�");
                }

                return Ok(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration du joueur {PlayerId}", id);
                return StatusCode(500, "Une erreur est survenue lors de la r�cup�ration du joueur");
            }
        }

        /// <summary>
        /// R�cup�re les joueurs d'une �quipe
        /// GET: api/Player/team/5
        /// </summary>
        [HttpGet("team/{teamId}")]
        [ProducesResponseType(typeof(IEnumerable<PlayerDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayersByTeam(int teamId)
        {
            try
            {
                if (teamId <= 0)
                {
                    return BadRequest("L'identifiant de l'�quipe doit �tre sup�rieur � z�ro");
                }

                var players = await _playerService.GetPlayersByTeamAsync(teamId);

                if (players == null || !players.Any())
                {
                    _logger.LogInformation("Aucun joueur trouv� pour l'�quipe {TeamId}", teamId);
                    return Ok(new List<PlayerDto>());
                }

                return Ok(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration des joueurs de l'�quipe {TeamId}", teamId);
                return StatusCode(500, "Une erreur est survenue lors de la r�cup�ration des joueurs");
            }
        }

        /// <summary>
        /// Cr�e un nouveau joueur
        /// POST: api/Player
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator,Encoder")]
        [ProducesResponseType(typeof(PlayerDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PlayerDto>> CreatePlayer([FromBody] CreatePlayerDto createPlayerDto)
        {
            try
            {
                if (createPlayerDto == null)
                {
                    return BadRequest("Les donn�es du joueur sont requises");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validation suppl�mentaire du num�ro de maillot
                if (createPlayerDto.JerseyNumber < MIN_JERSEY_NUMBER || createPlayerDto.JerseyNumber > MAX_JERSEY_NUMBER)
                {
                    return BadRequest($"Le num�ro de maillot doit �tre entre {MIN_JERSEY_NUMBER} et {MAX_JERSEY_NUMBER}");
                }

                var createdPlayer = await _playerService.CreatePlayerAsync(createPlayerDto);

                _logger.LogInformation("Joueur cr�� avec succ�s: {PlayerName} (#{JerseyNumber})",
                    $"{createPlayerDto.FirstName} {createPlayerDto.LastName}",
                    createPlayerDto.JerseyNumber);

                return CreatedAtAction(
                    nameof(GetPlayerById),
                    new { id = createdPlayer.Id },
                    createdPlayer);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "�chec de cr�ation du joueur - r�gle m�tier viol�e");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la cr�ation du joueur");
                return StatusCode(500, "Une erreur est survenue lors de la cr�ation du joueur");
            }
        }

        /// <summary>
        /// Met � jour un joueur existant
        /// PUT: api/Player/5
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Encoder")]
        [ProducesResponseType(typeof(PlayerDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PlayerDto>> UpdatePlayer(int id, [FromBody] UpdatePlayerDto updatePlayerDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("L'identifiant du joueur doit �tre sup�rieur � z�ro");
                }

                if (updatePlayerDto == null)
                {
                    return BadRequest("Les donn�es de mise � jour sont requises");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validation du num�ro de maillot si fourni
                if (updatePlayerDto.JerseyNumber.HasValue)
                {
                    if (updatePlayerDto.JerseyNumber.Value < MIN_JERSEY_NUMBER ||
                        updatePlayerDto.JerseyNumber.Value > MAX_JERSEY_NUMBER)
                    {
                        return BadRequest($"Le num�ro de maillot doit �tre entre {MIN_JERSEY_NUMBER} et {MAX_JERSEY_NUMBER}");
                    }
                }

                var updatedPlayer = await _playerService.UpdatePlayerAsync(id, updatePlayerDto);

                if (updatedPlayer == null)
                {
                    _logger.LogWarning("Tentative de mise � jour d'un joueur inexistant: {PlayerId}", id);
                    return NotFound($"Joueur avec l'ID {id} non trouv�");
                }

                _logger.LogInformation("Joueur mis � jour avec succ�s: {PlayerId}", id);
                return Ok(updatedPlayer);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "�chec de mise � jour du joueur - r�gle m�tier viol�e");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise � jour du joueur {PlayerId}", id);
                return StatusCode(500, "Une erreur est survenue lors de la mise � jour du joueur");
            }
        }

        /// <summary>
        /// Supprime un joueur
        /// DELETE: api/Player/5
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("L'identifiant du joueur doit �tre sup�rieur � z�ro");
                }

                var result = await _playerService.DeletePlayerAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Tentative de suppression d'un joueur inexistant: {PlayerId}", id);
                    return NotFound($"Joueur avec l'ID {id} non trouv�");
                }

                _logger.LogInformation("Joueur supprim� avec succ�s: {PlayerId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du joueur {PlayerId}", id);
                return StatusCode(500, "Une erreur est survenue lors de la suppression du joueur");
            }
        }

        /// <summary>
        /// D�finit les 5 joueurs de base pour un match
        /// POST: api/Player/starting-five
        /// </summary>
        [HttpPost("starting-five")]
        [Authorize(Roles = "Administrator,Encoder")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> SetStartingFive([FromBody] SetStartingFiveDto startingFiveDto)
        {
            try
            {
                if (startingFiveDto == null)
                {
                    return BadRequest("Les donn�es sont requises");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validation du nombre de joueurs
                if (startingFiveDto.PlayerIds.Count != STARTING_FIVE_COUNT)
                {
                    return BadRequest($"Exactement {STARTING_FIVE_COUNT} joueurs doivent �tre s�lectionn�s");
                }

                // V�rification des doublons
                if (startingFiveDto.PlayerIds.Distinct().Count() != STARTING_FIVE_COUNT)
                {
                    return BadRequest("Les joueurs doivent �tre uniques");
                }

                await _playerService.SetStartingFiveAsync(
                    startingFiveDto.MatchId,
                    startingFiveDto.TeamId,
                    startingFiveDto.PlayerIds);

                _logger.LogInformation("5 de base d�fini pour l'�quipe {TeamId} dans le match {MatchId}",
                    startingFiveDto.TeamId,
                    startingFiveDto.MatchId);

                return Ok(new { message = "5 de base d�fini avec succ�s" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "�chec de d�finition du 5 de base - r�gle m�tier viol�e");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la d�finition du 5 de base");
                return StatusCode(500, "Une erreur est survenue lors de la d�finition du 5 de base");
            }
        }

        /// <summary>
        /// R�cup�re les statistiques d'un joueur pour un match
        /// GET: api/Player/5/match/10/stats
        /// </summary>
        [HttpGet("{playerId}/match/{matchId}/stats")]
        [ProducesResponseType(typeof(PlayerMatchStatsDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<PlayerMatchStatsDto>> GetPlayerMatchStats(int playerId, int matchId)
        {
            try
            {
                if (playerId <= 0 || matchId <= 0)
                {
                    return BadRequest("Les identifiants doivent �tre sup�rieurs � z�ro");
                }

                var stats = await _playerService.GetPlayerMatchStatsAsync(playerId, matchId);

                if (stats == null)
                {
                    return NotFound("Statistiques non trouv�es pour ce joueur dans ce match");
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration des statistiques du joueur {PlayerId} pour le match {MatchId}",
                    playerId, matchId);
                return StatusCode(500, "Une erreur est survenue lors de la r�cup�ration des statistiques");
            }
        }
    }
}