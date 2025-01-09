using System.ComponentModel.DataAnnotations;

namespace PueblaApi.DTOS.Auth;

public class TokenRequest
{
    // JWT.
    [Required]
    public string Token { set; get; }
    // Last refresh token issued to user.
    [Required]
    public string RefreshToken { set; get; }
}

