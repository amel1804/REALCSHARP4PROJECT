using BasketballLiveScore.DTOs.User;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BasketballLiveScore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly ILoginService _loginService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

        // Constantes pour éviter les valeurs magiques
        private const string JWT_KEY_CONFIG = "Jwt:Key";
        private const string REGISTRATION_SUCCESS = "Utilisateur enregistré avec succès";
        private const string REGISTRATION_FAILED = "Échec de l'enregistrement : {0}";
        private const string INVALID_DATA = "Données invalides";
        private const string INVALID_CREDENTIALS = "Identifiants invalides";
        private const string JWT_KEY_ERROR = "Clé JWT non configurée";

        public AuthenticationController(
            IRegisterService registerService,
            ILoginService loginService,
            IConfiguration configuration,
            ILogger<AuthenticationController> logger)
        {
            _registerService = registerService ?? throw new ArgumentNullException(nameof(registerService));
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur dans le système
        /// </summary>
        /// <param name="userRegistrationDto">Informations d'enregistrement de l'utilisateur</param>
        /// <returns>Résultat de l'enregistrement</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto userRegistrationDto)
        {
            try
            {
                // Validation de base
                if (userRegistrationDto == null)
                {
                    _logger.LogWarning("Tentative d'enregistrement avec des données nulles");
                    return BadRequest(INVALID_DATA);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Tentative d'enregistrement avec des données invalides");
                    return BadRequest(ModelState);
                }

                // Appel au service d'enregistrement
                var result = await Task.Run(() => _registerService.Register(
                    userRegistrationDto.Username,
                    userRegistrationDto.Password,
                    userRegistrationDto.Role
                ));

                if (result == "OK")
                {
                    _logger.LogInformation($"Nouvel utilisateur enregistré : {userRegistrationDto.Username}");
                    return Ok(new { message = REGISTRATION_SUCCESS });
                }

                _logger.LogWarning($"Échec de l'enregistrement pour {userRegistrationDto.Username}: {result}");
                return BadRequest(new { message = string.Format(REGISTRATION_FAILED, result) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Une erreur est survenue lors de l'enregistrement" });
            }
        }

        /// <summary>
        /// Authentifie un utilisateur et génère un token JWT
        /// </summary>
        /// <param name="userLoginDto">Informations de connexion</param>
        /// <returns>Token JWT et informations utilisateur</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            try
            {
                // Validation de base
                if (userLoginDto == null)
                {
                    _logger.LogWarning("Tentative de connexion avec des données nulles");
                    return BadRequest(INVALID_DATA);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Tentative de connexion avec des données invalides");
                    return BadRequest(ModelState);
                }

                // Authentification
                var user = await Task.Run(() =>
                    _loginService.Login(userLoginDto.Username, userLoginDto.Password));

                if (user == null)
                {
                    _logger.LogWarning($"Échec de connexion pour : {userLoginDto.Username}");
                    return Unauthorized(new { message = INVALID_CREDENTIALS });
                }

                // Génération des claims pour le JWT
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                // Récupération de la clé JWT depuis la configuration
                var jwtKey = _configuration[JWT_KEY_CONFIG];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    _logger.LogError(JWT_KEY_ERROR);
                    throw new InvalidOperationException(JWT_KEY_ERROR);
                }

                // Génération du token
                var token = _loginService.GenerateToken(jwtKey, claims);

                _logger.LogInformation($"Connexion réussie pour : {user.Username}");

                // Retour des informations de connexion
                return Ok(new
                {
                    token = token,
                    username = user.Username,
                    role = user.Role,
                    userId = user.Id,
                    email = user.Email,
                    fullName = user.FullName,
                    expiresIn = 28800 // 8 heures en secondes
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Erreur de configuration");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Erreur de configuration du serveur" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Une erreur est survenue lors de la connexion" });
            }
        }

        /// <summary>
        /// Vérifie la validité d'un token JWT
        /// </summary>
        /// <returns>Informations de l'utilisateur si le token est valide</returns>
        [HttpGet("verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult VerifyToken()
        {
            // Vérification automatique par le middleware d'authentification
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok(new
                {
                    isValid = true,
                    username = User.Identity.Name,
                    role = User.FindFirst(ClaimTypes.Role)?.Value,
                    userId = User.FindFirst("UserId")?.Value
                });
            }

            return Unauthorized(new { isValid = false });
        }
    }
}