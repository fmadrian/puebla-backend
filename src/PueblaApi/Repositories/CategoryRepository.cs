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

    public async Task Delete(Category item)
    {
        // To delete the entity (not being tracked by context), we have to mark it as 'Deleted'.

        // Set entity's state as deleted, then remove it from context and save.
        this._context.Entry(item).State = EntityState.Deleted;

        this._context.Categories.Remove(item);
        int result = await this._context.SaveChangesAsync();
        if (result == 0)
            throw new ApiException("[REPOSITORY]: Couldn't add remove item from database.");
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
                                Name = movie.Studio.Name,
                                Id = movie.Studio.Id
                            }
                        }
                    ).ToList()
                }
            ).FirstOrDefaultAsync();
    }

    public async Task<SearchResponse<Category>> Search(SearchCategoryRequest request, bool includeRelated = true)
    {
        // 1. Build main query.
        IQueryable<Category> query = this._context.Categories
                            .Where(
                                item => (
                                    request.Q != null ? item.Name.ToLower().Contains(request.Q.ToLower()) : true
                                )
                            ).AsQueryable();

        // 2. Retrieve fields, and add related entities.

        query = query.Select(item => new Category()
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
                        Name = movie.Studio.Name,
                        Id = movie.Studio.Id
                    }
                }
            ).ToList()
        });

        // 3. Apply sorting column.
        Expression<Func<Category, object>> keySelector = request.SortColumn?.ToLower() switch
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
        var result = await SearchResponse<Category>.CreateAsync(query, request.Page, request.PageSize);
        return result;
    }

    public async Task<Category> Update(Category item)
    {
        // After being retrieved entity is not loaded into context.
        // To indicate changes took place and an update should happen, we have to mark
        // the entity's state as 'Modified'.

        this._context.Entry(item).State = EntityState.Modified;

        var result = await this._context.SaveChangesAsync();
        if (result == 0)
            throw new ApiInternalException("[REPOSITORY]: Couldn't update item in database.");
        return item;

        // Per: https://learn.microsoft.com/en-us/ef/ef6/saving/change-tracking/entity-state
        // An alternative to the previous process is: 
        //      1. Attach the updated entity (not tracked) to the context.
        //      2. Save the context.

        // this._context.Attach(item);
        // var result = await this._context.SaveChangesAsync();
        // ...   
    }
}
