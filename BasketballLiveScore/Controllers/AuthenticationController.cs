using BasketballLiveScore.DTOs;
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
                userRegistrationDTO.Name ?? throw new ArgumentNullException(nameof(userRegistrationDTO.Name)),
                userRegistrationDTO.Password ?? throw new ArgumentNullException(nameof(userRegistrationDTO.Password)),
                userRegistrationDTO.Role ?? throw new ArgumentNullException(nameof(userRegistrationDTO.Role))
            );

            if (result == "OK")
                return Ok("User registered successfully");

            return BadRequest("Registration failed");
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserConnectionDto userConnectionDTO)
        {
            if (userConnectionDTO == null)
                return BadRequest("Invalid login data.");

            var user = _loginService.Login(userConnectionDTO.Name, userConnectionDTO.Password);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
            var token = _loginService.GenerateToken(jwtKey, claims);

            return Ok(new { Token = token });
        }
    }
}
