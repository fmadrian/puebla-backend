using System;
using PueblaApi.DTOS.Base;

namespace PueblaApi.DTOS.Category;

public class SearchCategoryRequest : SearchRequest
{
    public string? Q { get; set; } // Q = query.
}
