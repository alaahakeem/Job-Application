using JobApplication.Application.Common;
using JobApplication.Application.DTOs.Auth;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RegisterCommandHandler(IIdentityService identityService, IJwtTokenGenerator jwtTokenGenerator)
        {
            _identityService = identityService;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var created = await _identityService.CreateUserAsync(
                request.FullName, request.Email, request.Password, request.Role);

            if (!created.Succeeded)
                return Result<AuthResponseDto>.Failure(created.Errors);

            var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(created.Value!);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                User = created.Value!
            });
        }
    }
}
