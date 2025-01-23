using System.ComponentModel.DataAnnotations;

namespace PueblaApi.DTOS.Auth;

// Signup requests.
public class SignupRequest
{
    [Required]
    public string FirstName { set; get; } // User's first name.
    [Required]
    public string LastName { set; get; } // User's last name.
    // [Required]
    // public string NationalId { set; get; } // User's national ID.
    [Required]
    public string Email { set; get; } // Email where we will send the recovery password.
    [Required]
    public string Username { set; get; } // Username for this user.
    [Required]
    public string Password { set; get; }
    [Required]
    public string Role { set; get; } // Users can only have one role.
}
