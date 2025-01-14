namespace PueblaApi.Settings;

public class JwtConfiguration
{
    public string Secret { set; get; }
    public string ExpiryTimeFrame { set; get; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
}