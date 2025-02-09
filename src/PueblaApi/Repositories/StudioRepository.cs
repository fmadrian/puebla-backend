using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PueblaApi.Database;
using PueblaApi.DTOS.Base;
using PueblaApi.DTOS.Studio;
using PueblaApi.Entities;
using PueblaApi.Exceptions;
using PueblaApi.Repositories.Interfaces;

namespace PueblaApi.Repositories;

public class StudioRepository : IStudioRepository
{

    #region Dependency injections
    private readonly ApplicationDbContext _context;


    public StudioRepository(ApplicationDbContext context)
    {
        this._context = context;
    }
    #endregion

    public async Task<bool> Any(Expression<Func<Studio, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await this._context.Studios.AnyAsync(predicate, cancellationToken);
    }

    public async Task<Studio> Create(Studio item)
    {
        this._context.Studios.Add(item);
        var result = await this._context.SaveChangesAsync();
        if (result == 0)
            throw new ApiInternalException("[REPOSITORY]: Couldn't add item to database.");
        return item;
    }

    public async Task Delete(Studio item)
    {
        // To delete the entity (not being tracked by context), we have to mark it as 'Deleted'.
        // Set entity's state as deleted, then remove it from context and save.
        this._context.Entry(item).State = EntityState.Deleted;
        this._context.Studios.Remove(item);
        int result = await this._context.SaveChangesAsync();
        if (result == 0)
            throw new ApiException("[REPOSITORY]: Couldn't add remove item from database.");
    }

    public async Task<Studio?> GetById(long id)
    {
        // 1. Build and execute query.
        // Using Select to grab only the fields we need.
        // Project related fields to avoid loading them in context.
        return await this._context.Studios
            .Where(item => item.Id == id)
            .Select(item => new Studio
            {
                Id = item.Id,
                Name = item.Name,
                Country = item.Country,
                FoundationYear = item.FoundationYear,
                Movies = item.Movies.Select(movie => new Movie()
                {
                    Id = movie.Id,
                    BoxOffice = movie.BoxOffice,
                    Name = movie.Name,
                    ImageURL = movie.ImageURL,
                    ReleaseYear = movie.ReleaseYear,
                    Categories = movie.Categories.Select(
                        category => new Category
                        {
                            Id = category.Id,
                            Name = category.Name
                        }
                    ).ToList(),
                    Studio = null
                }).ToList()
            }).FirstOrDefaultAsync();
    }

    public async Task<SearchResponse<Studio>> Search(SearchStudioRequest request, bool includeRelated = true)
    {
        // 1. Build main query.
        IQueryable<Studio> query = this._context.Studios
                            .Where(
                                item => (
                                    request.Q != null ? item.Name.ToLower().Contains(request.Q.ToLower()) : true
                                )
                            ).AsQueryable();

        // 2. Retrieve fields, and add related entities.

        query = query.Select(item => new Studio
        {
            Id = item.Id,
            Name = item.Name,
            Country = item.Country,
            FoundationYear = item.FoundationYear,
            Movies = item.Movies.Select(movie => new Movie()
            {
                Id = movie.Id,
                BoxOffice = movie.BoxOffice,
                Name = movie.Name,
                ImageURL = movie.ImageURL,
                ReleaseYear = movie.ReleaseYear,
                Categories = includeRelated ? new List<Category>() { } : movie.Categories.Select(
                    category => new Category
                    {
                        Id = category.Id,
                        Name = category.Name
                    }
                ).ToList(),
                Studio = null
            }).ToList()
        });


        // 3. Apply sorting column.
        Expression<Func<Studio, object>> keySelector = request.SortColumn?.ToLower() switch
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
        var result = await SearchResponse<Studio>.CreateAsync(query, request.Page, request.PageSize);
        return result;
    }

    public async Task<Studio> Update(Studio item)
    {
        // After being retrieved entity is not loaded into context.
        // To indicate changes took place and an update should happen, we have to mark
        // the entity's state as 'Modified'.

        this._context.Entry(item).State = EntityState.Modified;

        var result = await this._context.SaveChangesAsync();
        if (result == 0)
            throw new ApiInternalException("[REPOSITORY]: Couldn't update item in database.");
        return item;
    }

}
