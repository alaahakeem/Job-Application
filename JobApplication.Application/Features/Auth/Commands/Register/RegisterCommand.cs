using System.ComponentModel.DataAnnotations;
using JobApplication.Application.Common;
using JobApplication.Application.DTOs.Auth;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<Result<AuthResponseDto>>
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required, RegularExpression($"^({Roles.Candidate}|{Roles.Recruiter})$",
            ErrorMessage = "Role must be Candidate or Recruiter.")]
        public string Role { get; set; } = string.Empty;
    }
}
