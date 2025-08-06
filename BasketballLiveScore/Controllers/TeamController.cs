using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BasketballLiveScore.DTOs.Team;
using BasketballLiveScore.DTOs.Player;
using BasketballLiveScore.Services.Interfaces;

namespace BasketballLiveScore.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des équipes
    /// Suit le pattern MVC vu dans les notes de cours
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        /// <summary>
        /// Constructeur avec injection de dépendance
        /// Comme vu dans VideoGameManager et les notes sur DI
        /// </summary>
        public TeamController(ITeamService teamService)
        {
            _teamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
        }

        /// <summary>
        /// Récupère toutes les équipes
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TeamDto>), 200)]
        public async Task<IActionResult> GetAllTeams()
        {
            try
            {
                var teams = await _teamService.GetAllTeamsAsync();
                return Ok(teams);
            }
            catch (Exception ex)
            {
                // Log l'erreur en production
                return StatusCode(500, "Une erreur est survenue lors de la récupération des équipes");
            }
        }

        /// <summary>
        /// Récupère une équipe par son identifiant
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TeamDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTeamById(int id)
        {
            try
            {
                var team = await _teamService.GetTeamByIdAsync(id);

                if (team == null)
                    return NotFound($"Équipe avec l'ID {id} non trouvée");

                return Ok(team);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Une erreur est survenue lors de la récupération de l'équipe");
            }
        }

        /// <summary>
        /// Récupère les joueurs d'une équipe
        /// </summary>
        [HttpGet("{id}/players")]
        [ProducesResponseType(typeof(IEnumerable<PlayerSummaryDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTeamPlayers(int id)
        {
            try
            {
                var players = await _teamService.GetTeamPlayersAsync(id);

                if (players == null)
                    return NotFound($"Équipe avec l'ID {id} non trouvée");

                return Ok(players);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Une erreur est survenue lors de la récupération des joueurs");
            }
        }

        /// <summary>
        /// Crée une nouvelle équipe
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(TeamDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto createTeamDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdTeam = await _teamService.CreateTeamAsync(createTeamDto);
                return CreatedAtAction(nameof(GetTeamById), new { id = createdTeam.Id }, createdTeam);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Une erreur est survenue lors de la création de l'équipe");
            }
        }

        /// <summary>
        /// Met à jour une équipe existante
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(TeamDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateTeam(int id, [FromBody] UpdateTeamDto updateTeamDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedTeam = await _teamService.UpdateTeamAsync(id, updateTeamDto);

                if (updatedTeam == null)
                    return NotFound($"Équipe avec l'ID {id} non trouvée");

                return Ok(updatedTeam);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Une erreur est survenue lors de la mise à jour de l'équipe");
            }
        }

        /// <summary>
        /// Supprime une équipe
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            try
            {
                var result = await _teamService.DeleteTeamAsync(id);

                if (!result)
                    return NotFound($"Équipe avec l'ID {id} non trouvée");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Une erreur est survenue lors de la suppression de l'équipe");
            }
        }
    }
}