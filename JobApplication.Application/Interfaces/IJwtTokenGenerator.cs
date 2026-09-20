using JobApplication.Application.DTOs.Auth;

namespace JobApplication.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime ExpiresAtUtc) GenerateToken(AuthUserDto user);
    }
}
