namespace JobApplication.Application.DTOs.Auth
{
    /// <summary>The safe-to-share view of a user (never contains the password hash).</summary>
    public class AuthUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
