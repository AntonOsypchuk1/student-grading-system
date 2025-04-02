namespace SGS.API.Settings;

public class JwtSettings
{
    public string Secret { get; set; } = "MY_SUPER_SECRET_KEY_SHOULD_BE_LONG_AND_SECURE";
    public int ExpirationInMinutes { get; set; } = 60;
    public string Issuer { get; set; } = "SGS_API";
    public string Audience { get; set; } = "SGS_CLIENTS";
}