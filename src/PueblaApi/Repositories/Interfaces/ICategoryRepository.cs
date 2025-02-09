using System;
using PueblaApi.DTOS.Base;
using PueblaApi.DTOS.Category;
using PueblaApi.Entities;

namespace PueblaApi.Repositories.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetById(long id);
    Task<SearchResponse<Category>> Search(SearchCategoryRequest request, bool includeRelated = true);
}
