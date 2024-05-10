namespace src;

public class Jwt
{
    public string Secret { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; set; } = null!;
    public int DurationInHours { get; init; }
}