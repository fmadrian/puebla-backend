using PueblaApi.DTOS.Base;

namespace PueblaApi.Dtos.User;

public class SearchUserRequest : SearchRequest
{
#nullable enable
    public string? Q { get; set; } // Q = query.
    /// <summary>
    /// Search disabled accounts.
    /// </summary>
    public bool? IsEnabled { get; set; } = true;
#nullable disable
}