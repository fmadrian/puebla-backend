using Microsoft.EntityFrameworkCore;

namespace PueblaApi.DTOS.Base;
/// <summary>
/// Represents the result of a search query.
/// </summary>
/// <typeparam name="T">Type of objects to be returned in Items field.</typeparam>
public class SearchResponse<T>
{
    // Items.
    public List<T> Items { get; set; }
    // Current page.
    public int Page { get; set; }
    public int PageSize { get; set; }
    /// Total of items in query.
    public long TotalCount { get; set; }
    public bool HasNextPage => (Page * PageSize) < TotalCount;
    public bool HasPreviousPage => Page > 1;

    public SearchResponse(List<T> Items, int Page, int PageSize, long TotalCount)
    {
        this.Items = Items;
        this.Page = Page;
        this.PageSize = PageSize;
        this.TotalCount = TotalCount;
    }
    public SearchResponse(List<T> Items, SearchResponse<T> previousSearch)
    {
        this.Items = Items;
        this.Page = previousSearch.Page;
        this.PageSize = previousSearch.PageSize;
        this.TotalCount = previousSearch.TotalCount;
    }
    /// <summary>
    /// Takes a list of a different type and builds a new SearchResponse object using 
    /// the same data plus the new list of items.
    /// </summary>
    /// <typeparam name="E">New type of item the search response will carry</typeparam>
    /// <param name="items">List of items to be returned</param>
    /// <returns>A search response of the new type </returns>
    public SearchResponse<E> ConvertResponse<E>(List<E> items)
    {
        return new SearchResponse<E>(items, this.Page, this.PageSize, this.TotalCount);
    }

    /// <summary>
    /// Factory method to execute a paginated query.
    /// IMPORTANT: Sort order, sort fields, where fields MUST be defined BEFORE calling this method.
    /// </summary>
    /// <param name="query">Query to be executed</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Elements on page</param>
    /// <returns>A sorted list of elements according to query, page, page size.</returns>
    public static async Task<SearchResponse<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
    {
        // Build search response object that will execute the query and return additional information.

        // 1. Execute a query that returns the amount of rows in the query.
        int totalCount = await query.CountAsync();
        // 2. Get paginated items.
        var items = await query
                        .Skip((page - 1) * pageSize) // Skip first N (size * page) records.
                        .Take(pageSize) // Take N (size) records.
                        .ToListAsync();
        // 3. Create response object and return it.
        return new SearchResponse<T>(items, page, pageSize, totalCount);
    }
}