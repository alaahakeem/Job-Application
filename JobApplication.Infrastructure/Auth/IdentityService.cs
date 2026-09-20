using JobApplication.Application.Common;
using JobApplication.Application.DTOs.Auth;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Auth
{
    public class IdentityService : IIdentityService
    {
        // One generic message for every login failure, so attackers can't
        // discover which emails are registered.
        private const string InvalidCredentialsMessage = "Invalid email or password.";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<Result<AuthUserDto>> CreateUserAsync(string fullName, string email, string password, string role)
        {
            if (role != Roles.Candidate && role != Roles.Recruiter)
                return Result<AuthUserDto>.Failure("Role must be Candidate or Recruiter.");

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
                return Result<AuthUserDto>.Failure("Email is already registered.");

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName
            };

            // user + role + candidate profile: all or nothing (leaving without CommitAsync rolls back).
            await using var tx = await _context.Database.BeginTransactionAsync();

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return Result<AuthUserDto>.Failure(result.Errors.Select(e => e.Description));

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
                return Result<AuthUserDto>.Failure(roleResult.Errors.Select(e => e.Description));

            if (role == Roles.Candidate)
            {
                _context.Candidates.Add(new Candidate { Name = fullName, CvUrl = string.Empty, UserId = user.Id });
                await _context.SaveChangesAsync();
            }

            await tx.CommitAsync();

            return Result<AuthUserDto>.Success(ToDto(user, role));
        }

        public async Task<Result<AuthUserDto>> CheckCredentialsAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Result<AuthUserDto>.Failure(InvalidCredentialsMessage);

            // Too many wrong passwords => locked for a few minutes (see lockout options in Program.cs).
            if (await _userManager.IsLockedOutAsync(user))
                return Result<AuthUserDto>.Failure(InvalidCredentialsMessage);

            var passwordIsValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordIsValid)
            {
                await _userManager.AccessFailedAsync(user);
                return Result<AuthUserDto>.Failure(InvalidCredentialsMessage);
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            return Result<AuthUserDto>.Success(ToDto(user, roles.FirstOrDefault() ?? string.Empty));
        }

        private static AuthUserDto ToDto(ApplicationUser user, string role)
        {
            return new AuthUserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = role
            };
        }
    }
}
