
namespace PueblaApi.DTOS.Auth;

// Response to signup and refresh token requests.
public class AuthResponse
{
    public string Token { set; get; }
    public string RefreshToken { set; get; }
    /// <summary>
    /// List of roles.
    /// </summary>
    public List<string> Roles { set; get; }

    #region Other fields
    public string FirstName { set; get; }
    public string LastName { set; get; }
    #endregion



}
