using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PueblaApi.Database;
using PueblaApi.DTOS.Base;
using PueblaApi.DTOS.Movie;
using PueblaApi.Entities;
using PueblaApi.Exceptions;
using PueblaApi.Repositories.Interfaces;

namespace PueblaApi.Repositories;

public class MovieRepository : IMovieRepository
{

    #region Dependency injections
    private readonly ApplicationDbContext _context;


    public MovieRepository(ApplicationDbContext context)
    {
        this._context = context;
    }
    #endregion

    public async Task<bool> Any(Expression<Func<Movie, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await this._context.Movies.AnyAsync(predicate, cancellationToken);
    }

    public async Task<Movie> Create(Movie item)
    {
        // Indicate that no change should be made to the ASSOCIATED entities.
        foreach (Category relatedItem in item.Categories)
        {
            this._context.Entry(relatedItem).State = EntityState.Unchanged;
        }
        if (item.Studio != null)
        {
            this._context.Entry(item.Studio).State = EntityState.Unchanged;
        }

        this._context.Movies.Add(item);
        var result = await this._context.SaveChangesAsync();
        if (result == 0)
            throw new ApiInternalException("[REPOSITORY]: Couldn't add item to database.");
        return item;
    }

    public async Task Delete(Movie item)
    {
        // To delete the entity (not being tracked by context), we have to mark it as 'Deleted'.

        // Set entity's state as deleted, then remove it from context and save.
        this._context.Entry(item).State = EntityState.Deleted;

        this._context.Movies.Remove(item);
        int result = await this._context.SaveChangesAsync();
        if (result == 0)
            throw new ApiException("[REPOSITORY]: Couldn't add remove item from database.");
    }

    public async Task<Movie?> GetById(long id)
    {
        // 1. Build and execute query.
        // Using Select to grab only the fields we need.
        // Studio and Categories ignore field movies in order to avoid an endless loop.
        return await this._context.Movies
            .Where(item => item.Id == id)
            .Select(item => new Movie
            {
                Id = item.Id,
                Name = item.Name,
                BoxOffice = item.BoxOffice,
                ReleaseYear = item.ReleaseYear,
                ImageURL = item.ImageURL,
                Studio = item.Studio == null ? null : new Studio
                {
                    Id = item.Studio.Id,
                    Country = item.Studio.Country,
                    FoundationYear = item.Studio.FoundationYear,
                    Name = item.Studio.Name
                },
                Categories = item.Categories.Select(relItem => new Category
                {
                    Id = relItem.Id,
                    Name = relItem.Name
                }).ToList()
            }).FirstOrDefaultAsync();
    }

    public async Task<SearchResponse<Movie>> Search(SearchMovieRequest request, bool includeRelated = true)
    {
        // 1. Build main query.
        IQueryable<Movie> query = this._context.Movies
                            .Where(
                                item => (
                                    request.Q != null ? item.Name.ToLower().Contains(request.Q.ToLower()) : true
                                )
                                // If we are looking for studio, include it in the query and don't include movies with no studios.
                                && (
                                    request.Studio != null ? item.Studio != null && item.Studio.Id == request.Studio : true
                                )
                                // Include ALL categories passed as parameter, if there's any. Otherwise, include all movies.
                                && (
                                    request.Categories != null && request.Categories.Count() > 0 ?
                                    request.Categories.All(id => item.Categories.Select(c => c.Id).Contains(id))
                                    : true
                                )
                            ).AsQueryable();

        // 2. Retrieve fields, and add related entities.

        query = query.Select(
            item => new Movie
            {
                Id = item.Id,
                Name = item.Name,
                BoxOffice = item.BoxOffice,
                ReleaseYear = item.ReleaseYear,
                ImageURL = item.ImageURL,
                Studio = includeRelated && item.Studio != null ? new Studio
                {
                    Id = item.Studio.Id,
                    Country = item.Studio.Country,
                    FoundationYear = item.Studio.FoundationYear,
                    Name = item.Studio.Name
                } : null,
                Categories = includeRelated ? item.Categories.Select(relItem => new Category
                {
                    Id = relItem.Id,
                    Name = relItem.Name
                }).ToList() : new() { }
            }
        );


        // 3. Apply sorting column.
        Expression<Func<Movie, object>> keySelector = request.SortColumn?.ToLower() switch
        {
            "name" => x => x.Name,
            _ => x => x.Id // Default.
        };

        // 4. Apply sorting order (descending or ascending)
        if (request.SortOrder?.ToLower() == "desc")
            query = query.OrderByDescending(keySelector);
        else
            query = query.OrderBy(keySelector);

        // 5. Build search response object that will execute the query and return additional information.
        var result = await SearchResponse<Movie>.CreateAsync(query, request.Page, request.PageSize);
        return result;
    }

    public async Task<Movie> Update(Movie item)
    {
        // We don't need to change the related items entity state to 'Unchanged'
        // as they are not loaded into context.

        foreach (Category relatedItem in item.Categories)
        {
            this._context.Entry(relatedItem).State = EntityState.Unchanged;
        }
        if (item.Studio != null)
        {
            this._context.Entry(item.Studio).State = EntityState.Unchanged;
        }
        // No need to mark entity as 'Updated' as it is done automatically (entity already attached to context).
        var result = await this._context.SaveChangesAsync();
        if (result == 0)
            throw new ApiInternalException("[REPOSITORY]: Couldn't update item in database.");
        return item;
    }

    public async Task<Movie?> GetById_UseContext(long id)
    {
        // 1. Build and execute query.
        // Using Select to grab only the fields we need.
        // Studio and Categories ignore field movies in order to avoid an endless loop.
        return await this._context.Movies
            .Where(item => item.Id == id)
            .Include(item => item.Studio)
            .Include(item => item.Categories)
            .FirstOrDefaultAsync();
    }
}
