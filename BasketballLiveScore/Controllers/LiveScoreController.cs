using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BasketballLiveScore.DTOs.LiveScore;
using BasketballLiveScore.Services.Interfaces;
using BasketballLiveScore.Hubs;
using Microsoft.Extensions.Logging;

namespace BasketballLiveScore.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion du score en temps réel
    /// Gère toutes les actions pendant un match en cours
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator,Encoder")]
    public class LiveScoreController : ControllerBase
    {
        // Constantes pour les règles du basketball
        private const int MIN_QUARTER = 1;
        private const int MAX_QUARTER_STANDARD = 4;
        private const int MAX_QUARTER_WITH_OVERTIME = 10;
        private const int POINTS_FREE_THROW = 1;
        private const int POINTS_FIELD_GOAL = 2;
        private const int POINTS_THREE_POINTER = 3;
        private const int MAX_PERSONAL_FOULS = 5;
        private const int MAX_TIMEOUTS_PER_HALF = 3;

        private readonly ILiveScoreService _liveScoreService;
        private readonly IHubContext<LiveScoreHub> _hubContext;
        private readonly ILogger<LiveScoreController> _logger;

        /// <summary>
        /// Constructeur avec injection de dépendances
        /// </summary>
        public LiveScoreController(
            ILiveScoreService liveScoreService,
            IHubContext<LiveScoreHub> hubContext,
            ILogger<LiveScoreController> logger)
        {
            _liveScoreService = liveScoreService ?? throw new ArgumentNullException(nameof(liveScoreService));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Récupère le score en temps réel d'un match
        /// GET: api/LiveScore/match/5
        /// </summary>
        [HttpGet("match/{matchId}")]
        [AllowAnonymous] // Permet aux spectateurs de voir le score
        [ProducesResponseType(typeof(LiveScoreUpdateDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<LiveScoreUpdateDto>> GetLiveScore(int matchId)
        {
            try
            {
                if (matchId <= 0)
                {
                    return BadRequest("L'identifiant du match doit être supérieur à zéro");
                }

                var liveScore = await _liveScoreService.GetLiveScoreAsync(matchId);

                if (liveScore == null)
                {
                    return NotFound($"Match {matchId} non trouvé ou pas en cours");
                }

                return Ok(liveScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du score en direct pour le match {MatchId}", matchId);
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        /// <summary>
        /// Démarre le chronomètre du match
        /// POST: api/LiveScore/match/5/clock/start
        /// </summary>
        [HttpPost("match/{matchId}/clock/start")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> StartClock(int matchId)
        {
            try
            {
                if (matchId <= 0)
                {
                    return BadRequest("L'identifiant du match doit être supérieur à zéro");
                }

                var success = await _liveScoreService.StartClockAsync(matchId);

                if (!success)
                {
                    return NotFound($"Match {matchId} non trouvé ou impossible à démarrer");
                }

                // Notifier tous les clients via SignalR
                await _hubContext.Clients.Group($"match-{matchId}")
                    .SendAsync("ClockStarted", matchId);

                _logger.LogInformation("Chronomètre démarré pour le match {MatchId}", matchId);
                return Ok(new { message = "Chronomètre démarré" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du démarrage du chronomètre pour le match {MatchId}", matchId);
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        /// <summary>
        /// Arrête le chronomètre du match
        /// POST: api/LiveScore/match/5/clock/stop
        /// </summary>
        [HttpPost("match/{matchId}/clock/stop")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> StopClock(int matchId)
        {
            try
            {
                if (matchId <= 0)
                {
                    return BadRequest("L'identifiant du match doit être supérieur à zéro");
                }

                var success = await _liveScoreService.StopClockAsync(matchId);

                if (!success)
                {
                    return NotFound($"Match {matchId} non trouvé");
                }

                // Notifier tous les clients via SignalR
                await _hubContext.Clients.Group($"match-{matchId}")
                    .SendAsync("ClockStopped", matchId);

                _logger.LogInformation("Chronomètre arrêté pour le match {MatchId}", matchId);
                return Ok(new { message = "Chronomètre arrêté" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'arrêt du chronomètre pour le match {MatchId}", matchId);
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        /// <summary>
        /// Enregistre un panier marqué
        /// POST: api/LiveScore/match/5/basket
        /// </summary>
        [HttpPost("match/{matchId}/basket")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RecordBasket(int matchId, [FromBody] BasketScoreDto basketDto)
        {
            try
            {
                if (matchId <= 0)
                {
                    return BadRequest("L'identifiant du match doit être supérieur à zéro");
                }

                if (basketDto == null)
                {
                    return BadRequest("Les données du panier sont requises");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validation des points
                if (basketDto.Points != POINTS_FREE_THROW &&
                    basketDto.Points != POINTS_FIELD_GOAL &&
                    basketDto.Points != POINTS_THREE_POINTER)
                {
                    return BadRequest($"Les points doivent être {POINTS_FREE_THROW}, {POINTS_FIELD_GOAL} ou {POINTS_THREE_POINTER}");
                }

                // Validation du quart-temps
                if (basketDto.Quarter < MIN_QUARTER || basketDto.Quarter > MAX_QUARTER_WITH_OVERTIME)
                {
                    return BadRequest($"Le quart-temps doit être entre {MIN_QUARTER} et {MAX_QUARTER_WITH_OVERTIME}");
                }

                var success = await _liveScoreService.RecordBasketAsync(matchId, basketDto);

                if (!success)
                {
                    return NotFound($"Match {matchId} non trouvé ou pas en cours");
                }

                // Notifier tous les clients via SignalR
                var liveScore = await _liveScoreService.GetLiveScoreAsync(matchId);
                await _hubContext.Clients.Group($"match-{matchId}")
                    .SendAsync("UpdateScore", liveScore);

                _logger.LogInformation("Panier de {Points} points enregistré pour le joueur {PlayerId} au match {MatchId}",
                    basketDto.Points, basketDto.PlayerId, matchId);

                return Ok(new { message = $"Panier de {basketDto.Points} points enregistré" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement du panier pour le match {MatchId}", matchId);
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        /// <summary>
        /// Enregistre une faute
        /// POST: api/LiveScore/match/5/foul
        /// </summary>
        [HttpPost("match/{matchId}/foul")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RecordFoul(int matchId, [FromBody] FoulCommittedDto foulDto)
        {
            try
            {
                if (matchId <= 0)
                {
                    return BadRequest("L'identifiant du match doit être supérieur à zéro");
                }

                if (foulDto == null)
                {
                    return BadRequest("Les données de la faute sont requises");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validation du type de faute
                var validFoulTypes = new[] { "P0", "P1", "P2", "P3", "T", "U", "D" };
                if (!validFoulTypes.Contains(foulDto.FoulType.ToUpper()))
                {
                    return BadRequest($"Type de faute invalide. Types valides: {string.Join(", ", validFoulTypes)}");
                }

                var success = await _liveScoreService.RecordFoulAsync(matchId, foulDto);

                if (!success)
                {
                    return NotFound($"Match {matchId} non trouvé ou pas en cours");
                }

                // Notifier tous les clients via SignalR
                var liveScore = await _liveScoreService.GetLiveScoreAsync(matchId);
                await _hubContext.Clients.Group($"match-{matchId}")
                    .SendAsync("FoulCommitted", liveScore);

                _logger.LogInformation("Faute {FoulType} enregistrée pour le joueur {PlayerId} au match {MatchId}",
                    foulDto.FoulType, foulDto.PlayerId, matchId);

                return Ok(new { message = $"Faute {foulDto.FoulType} enregistrée" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de la faute pour le match {MatchId}", matchId);
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        /// <summary>
        /// Enregistre un changement de joueur
        /// POST: api/LiveScore/match/5/substitution
        /// </summary>
        [HttpPost("match/{matchId}/substitution")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RecordSubstitution(int matchId, [FromBody] PlayerSubstitutionDto substitutionDto)
        {
            try
            {
                if (matchId <= 0)
                {
                    return BadRequest("L'identifiant du match doit être supérieur à zéro");
                }

                if (substitutionDto == null)
                {
                    return BadRequest("Les données de substitution sont requises");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validation : joueurs différents
                if (substitutionDto.PlayerInId == substitutionDto.PlayerOutId)
                {
                    return BadRequest("Le joueur entrant et sortant doivent être différents");
                }

                var success = await _liveScoreService.RecordSubstitutionAsync(matchId, substitutionDto);

                if (!success)
                {
                    return NotFound($"Match {matchId} non trouvé ou substitution impossible");
                }

                // Notifier tous les clients via SignalR
                var liveScore = await _liveScoreService.GetLiveScoreAsync(matchId);
                await _hubContext.Clients.Group($"match-{matchId}")
                    .SendAsync("PlayerSubstitution", liveScore.TeamsOnCourt);

                _logger.LogInformation("Substitution: joueur {PlayerOut} remplacé par {PlayerIn} au match {MatchId}",
                    substitutionDto.PlayerOutId, substitutionDto.PlayerInId, matchId);

                return Ok(new { message = "Substitution enregistrée" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Substitution invalide pour le match {MatchId}", matchId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de la substitution pour le match {MatchId}", matchId);
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        /// <summary>
        /// Enregistre un timeout
        /// POST: api/LiveScore/match/5/timeout
        /// </summary>
        [HttpPost("match/{matchId}/timeout")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RecordTimeout(int matchId, [FromBody] TimeoutCalledDto timeoutDto)
        {
            try
            {
                if (matchId <= 0)
                {
                    return BadRequest("L'identifiant du match doit être supérieur à zéro");
                }

                if (timeoutDto == null)
                {
                    return BadRequest("Les données du timeout sont requises");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _liveScoreService.RecordTimeoutAsync(matchId, timeoutDto.TeamId);

                if (!success)
                {
                    return NotFound($"Match {matchId} non trouvé ou timeout impossible");
                }

                // Notifier tous les clients via SignalR
                await _hubContext.Clients.Group($"match-{matchId}")
                    .SendAsync("TimeoutCalled", timeoutDto.TeamId);

                _logger.LogInformation("Timeout appelé par l'équipe {TeamId} au match {MatchId}",
                    timeoutDto.TeamId, matchId);

                return Ok(new { message = "Timeout enregistré" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Timeout invalide pour le match {MatchId}", matchId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement du timeout pour le match {MatchId}", matchId);
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        /// <summary>
        /// Change de quart-temps
        /// POST: api/LiveScore/match/5/quarter
        /// </summary>
        [HttpPost("match/{matchId}/quarter")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangeQuarter(int matchId, [FromBody] ChangeQuarterDto quarterDto)
        {
            try
            {
                if (matchId <= 0)
                {
                    return BadRequest("L'identifiant du match doit être supérieur à zéro");
                }

                if (quarterDto == null)
                {
                    return BadRequest("Les données sont requises");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validation du quart-temps
                if (quarterDto.NewQuarter < MIN_QUARTER || quarterDto.NewQuarter > MAX_QUARTER_WITH_OVERTIME)
                {
                    return BadRequest($"Le quart-temps doit être entre {MIN_QUARTER} et {MAX_QUARTER_WITH_OVERTIME}");
                }

                // Logique pour changer de quart-temps (à implémenter dans le service)
                // Pour l'instant, on notifie juste via SignalR
                await _hubContext.Clients.Group($"match-{matchId}")
                    .SendAsync("QuarterChange", quarterDto.NewQuarter);

                _logger.LogInformation("Changement au quart-temps {Quarter} pour le match {MatchId}",
                    quarterDto.NewQuarter, matchId);

                return Ok(new { message = $"Passage au quart-temps {quarterDto.NewQuarter}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de quart-temps pour le match {MatchId}", matchId);
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        /// <summary>
        /// Met à jour le chronomètre manuellement
        /// PUT: api/LiveScore/match/5/clock
        /// </summary>
        [HttpPut("match/{matchId}/clock")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateGameClock(int matchId, [FromBody] UpdateGameClockDto clockDto)
        {
            try
            {
                if (matchId <= 0)
                {
                    return BadRequest("L'identifiant du match doit être supérieur à zéro");
                }

                if (clockDto == null)
                {
                    return BadRequest("Les données du chronomètre sont requises");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validation du temps (ne peut pas être négatif)
                if (clockDto.GameClockSeconds < 0)
                {
                    return BadRequest("Le temps ne peut pas être négatif");
                }

                // Notifier tous les clients via SignalR
                await _hubContext.Clients.Group($"match-{matchId}")
                    .SendAsync("UpdateClock", new GameClockDto
                    {
                        RemainingSeconds = clockDto.GameClockSeconds,
                        IsRunning = clockDto.IsRunning
                    });

                _logger.LogInformation("Chronomètre mis à jour: {Seconds}s, Running: {IsRunning} pour le match {MatchId}",
                    clockDto.GameClockSeconds, clockDto.IsRunning, matchId);

                return Ok(new { message = "Chronomètre mis à jour" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du chronomètre pour le match {MatchId}", matchId);
                return StatusCode(500, "Une erreur est survenue");
            }
        }
    }
}