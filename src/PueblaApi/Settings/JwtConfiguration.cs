namespace PueblaApi.Settings;

public class JwtConfiguration
{
    public string Secret { set; get; }
    public string ExpiryTimeFrame { set; get; }
}