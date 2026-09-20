namespace JobApplication.Application.DTOs.Auth
{
    /// <summary>What register/login return to the client.</summary>
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public AuthUserDto User { get; set; } = new();
    }
}
