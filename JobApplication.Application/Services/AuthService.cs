using JobApplication.Application.Common;
using JobApplication.Application.DTOs.Auth;
using JobApplication.Application.Interfaces;

namespace JobApplication.Application.Services
{
    public class AuthService
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IIdentityService identityService, IJwtTokenGenerator jwtTokenGenerator)
        {
            _identityService = identityService;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            var created = await _identityService.CreateUserAsync(
                registerDto.FullName, registerDto.Email, registerDto.Password, registerDto.Role);

            if (!created.Succeeded)
                return Result<AuthResponseDto>.Failure(created.Errors);

            return Result<AuthResponseDto>.Success(BuildResponse(created.Value!));
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            var checkedUser = await _identityService.CheckCredentialsAsync(loginDto.Email, loginDto.Password);

            if (!checkedUser.Succeeded)
                return Result<AuthResponseDto>.Failure(checkedUser.Errors);

            return Result<AuthResponseDto>.Success(BuildResponse(checkedUser.Value!));
        }

        private AuthResponseDto BuildResponse(AuthUserDto user)
        {
            var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                User = user
            };
        }
    }
}
