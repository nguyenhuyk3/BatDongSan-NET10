namespace VTTGROUP.Application.Auth.Dto
{
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public DateTime? ExpiresAt { get; set; }
    }
}
