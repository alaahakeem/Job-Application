using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace JobApplication.Infrastructure.Auth
{
    /// <summary>
    /// The user account stored by ASP.NET Core Identity (table AspNetUsers).
    /// Inherits Id, UserName, Email, PasswordHash, lockout fields, etc. from IdentityUser.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
    }
}
