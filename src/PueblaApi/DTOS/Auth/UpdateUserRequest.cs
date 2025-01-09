using System.ComponentModel.DataAnnotations;

namespace PueblaApi.DTOS.Auth;

public class UpdateUserRequest : UpdateAnyUserRequest
{
    // Current user's password.
    [Required]
    public string CurrentPassword { set; get; }
}