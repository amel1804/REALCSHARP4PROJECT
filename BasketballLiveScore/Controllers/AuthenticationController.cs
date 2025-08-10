using BasketballLiveScore.DTOs;
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

        public AuthenticationController(
            IRegisterService registerService,
            ILoginService loginService,
            IConfiguration configuration)
        {
            _registerService = registerService ?? throw new ArgumentNullException(nameof(registerService));
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody] UserRegistrationDto userRegistrationDTO)
        {
            if (userRegistrationDTO == null)
                return BadRequest("Invalid user data.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _registerService.Register(
                userRegistrationDTO.Username ?? throw new ArgumentNullException(nameof(userRegistrationDTO.Username)),
                userRegistrationDTO.Password ?? throw new ArgumentNullException(nameof(userRegistrationDTO.Password)),
                userRegistrationDTO.Role ?? throw new ArgumentNullException(nameof(userRegistrationDTO.Role))
            );

            if (result == "OK")
                return Ok("User registered successfully");

            return BadRequest($"Registration failed: {result}");
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserLoginDto userConnectionDTO)
        {
            if (userConnectionDTO == null)
                return BadRequest("Invalid login data.");

            var user = _loginService.Login(userConnectionDTO.Username, userConnectionDTO.Password);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),  // Utiliser Username au lieu de Name
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
            var token = _loginService.GenerateToken(jwtKey, claims);

            return Ok(new { Token = token, Username = user.Username, Role = user.Role });
        }
    }
}