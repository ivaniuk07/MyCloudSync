namespace MyCloudSync.Core.Models;

public class Account
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string? DisplayName { get; set; }
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public DateTime? TokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
