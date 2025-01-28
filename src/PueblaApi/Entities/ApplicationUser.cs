using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PueblaApi.Entities;

/// <summary>
/// Extends from the IdentityUser provided by Identity.
/// Adds profile data (Name and National ID) for application users by adding properties to the ApplicationUser class
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's first name.
    /// </summary>
    [Required]
    public string FirstName { set; get; }
    /// <summary>
    /// User's last name.
    /// </summary>
    [Required]
    public string LastName { set; get; }
    /// <summary>
    /// National ID. Unique for every user.
    /// </summary>
    // [Required]
    // public string NationalId { set; get; }
    [Required]
    public bool IsEnabled { set; get; }

    public EmailConfirmationCode? EmailConfirmationCode { get; set; }

}
