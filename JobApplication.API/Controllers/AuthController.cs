using System.Security.Claims;
using JobApplication.Application.Features.Auth.Commands.Login;
using JobApplication.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var result = await _mediator.Send(command);

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
