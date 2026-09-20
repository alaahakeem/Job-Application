using System.Security.Claims;
using JobApplication.Application.DTOs.Auth;
using JobApplication.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (!result.Succeeded)
                return Unauthorized(new { errors = result.Errors });

            return Ok(result.Value);
        }

        // Test endpoint: proves the token works. Requires a valid "Authorization: Bearer <token>" header.
        // You can delete it once you have real protected endpoints.
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                id = User.FindFirstValue("sub"),
                email = User.FindFirstValue("email"),
                fullName = User.FindFirstValue("name"),
                role = User.FindFirstValue("role")
            });
        }
    }
}
