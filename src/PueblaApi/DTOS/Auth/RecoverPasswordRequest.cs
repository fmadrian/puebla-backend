
using System.ComponentModel.DataAnnotations;

namespace PueblaApi.DTOS.Auth;

// Response to signup and refresh token requests.
public class RecoverPasswordRequest
{
    [Required]
    public string Email { set; get; }
    [Required]
    public string Username { set; get; }
}
