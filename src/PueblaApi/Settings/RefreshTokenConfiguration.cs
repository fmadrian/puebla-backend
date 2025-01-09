namespace PueblaApi.Settings;
/// <summary>
///  Helps to map the refresh tokens configuration.
/// </summary>
public class RefreshTokenConfiguration
{
    public int ExpiryDateDays { get; set; }
    public int TokenSize { get; set; }
}