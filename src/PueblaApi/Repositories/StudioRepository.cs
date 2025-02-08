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
        throw new NotImplementedException();
    }

    public async Task<Studio> Update(Studio item)
    {
        throw new NotImplementedException();
    }

}
