using JobApplication.Application.Common;
using JobApplication.Application.DTOs.Auth;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginCommandHandler(IIdentityService identityService, IJwtTokenGenerator jwtTokenGenerator)
        {
            _identityService = identityService;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var checkedUser = await _identityService.CheckCredentialsAsync(request.Email, request.Password);

            if (!checkedUser.Succeeded)
                return Result<AuthResponseDto>.Failure(checkedUser.Errors);

            var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(checkedUser.Value!);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                User = checkedUser.Value!
            });
        }
    }
}
