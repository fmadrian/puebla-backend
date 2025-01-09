using System.ComponentModel.DataAnnotations;

namespace PueblaApi.DTOS.Base;

public class SearchRequest
{
#nullable enable
    // Filtering.
    public string? SortColumn { get; set; }
    public string? SortOrder { get; set; } // Ascending (asc) or descending (desc).
#nullable disable

    // Page number.
    [Range(1, int.MaxValue)]
    public int Page { get; set; }
    // Page size.
    public int PageSize { get; set; }
}