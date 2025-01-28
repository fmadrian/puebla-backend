using System.ComponentModel.DataAnnotations;

namespace PueblaApi.DTOS.Auth;

public class UpdateAnyUserRequest
{
#nullable enable
    // New username.
    public string? Username { set; get; }
    // New email.
    public string? Email { set; get; }
    // New password.
    public string? Password { set; get; }
    // New first name.
    public string? FirstName { set; get; }
    // New last name.
    public string? LastName { set; get; }
    // New national ID.
    // public string? NationalId { set; get; }
    // New role.
    public string? Role { set; get; }
#nullable disable
}