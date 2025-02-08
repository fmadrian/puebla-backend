using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PueblaApi.Database;
using PueblaApi.DTOS.Base;
using PueblaApi.DTOS.Category;
using PueblaApi.Entities;
using PueblaApi.Exceptions;
using PueblaApi.Repositories.Interfaces;

namespace PueblaApi.Repositories;

public class CategoryRepository : ICategoryRepository
{
    #region Dependency injections
    private readonly ApplicationDbContext _context;


    public CategoryRepository(ApplicationDbContext context)
    {
        this._context = context;
    }
    #endregion

    public async Task<bool> Any(Expression<Func<Category, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await this._context.Categories.AnyAsync(predicate, cancellationToken);
    }

    public async Task<Category> Create(Category item)
    {
        this._context.Categories.Add(item);
        var result = await this._context.SaveChangesAsync();
        if (result == 0)
            throw new ApiInternalException("[REPOSITORY]: Couldn't add item to database.");
        return item;
    }

    public Task Delete(Category item)
    {
        throw new NotImplementedException();
    }

    public async Task<Category?> GetById(long id)
    {
        // 1. Build and execute query.
        // Using Select to grab only the fields we need.
        // Project related fields to avoid loading them in context.

        return await this._context.Categories
            .Where(item => item.Id == id)
            .Select(
                item => new Category()
                {
                    Id = item.Id,
                    Name = item.Name,
                    Movies = item.Movies.Select(
                        movie => new Movie()
                        {
                            Id = movie.Id,
                            Name = movie.Name,
                            ImageURL = movie.ImageURL,
                            BoxOffice = movie.BoxOffice,
                            ReleaseYear = movie.ReleaseYear,
                            Categories = movie.Categories.Select(
                                category => new Category()
                                {
                                    Id = category.Id,
                                    Name = category.Name
                                }
                            ).ToList(),
                            Studio = movie.Studio == null ? null : new Studio()
                            {
                                Name = movie.Name,
                                Id = movie.Id
                            }
                        }
                    ).ToList()
                }
            ).FirstOrDefaultAsync();
    }

    public Task<SearchResponse<Category>> Search(SearchCategoryRequest request, bool includeRelated = true)
    {
        throw new NotImplementedException();
    }

    public Task<Category> Update(Category item)
    {
        throw new NotImplementedException();
    }
}
