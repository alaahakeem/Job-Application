namespace JobApplication.Infrastructure.Auth
{
    /// <summary>
    /// Bound from the "Jwt" section of appsettings.json (SecretKey comes from user-secrets).
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 60;
    }
}
