using System.ComponentModel.DataAnnotations;
using JobApplication.Application.Common;
using JobApplication.Application.DTOs.Auth;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<Result<AuthResponseDto>>
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
