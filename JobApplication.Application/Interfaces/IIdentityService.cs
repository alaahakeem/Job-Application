using JobApplication.Application.Common;
using JobApplication.Application.DTOs.Auth;

namespace JobApplication.Application.Interfaces
{
    // Implemented in Infrastructure (ASP.NET Core Identity), so Application stays free of Identity/EF.
    public interface IIdentityService
    {
        Task<Result<AuthUserDto>> CreateUserAsync(string fullName, string email, string password, string role);
        Task<Result<AuthUserDto>> CheckCredentialsAsync(string email, string password);
    }
}
