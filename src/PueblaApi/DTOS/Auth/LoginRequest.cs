using System.ComponentModel.DataAnnotations;

namespace PueblaApi.DTOS.Auth;

// Login requests.
public class LoginRequest
{
    // Either username or email.
    [Required]
    public string Name { set; get; }
    [Required]
    public string Password { set; get; }
}