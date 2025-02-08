using System;
using PueblaApi.DTOS.Base;

namespace PueblaApi.DTOS.Studio;

public class SearchStudioRequest : SearchRequest
{
    public string? Q { get; set; } // Q = query.
}
