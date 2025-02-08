using System;
using PueblaApi.DTOS.Base;
using PueblaApi.DTOS.Studio;
using PueblaApi.Entities;

namespace PueblaApi.Repositories.Interfaces;

public interface IStudioRepository : IRepository<Studio>
{
    Task<Studio?> GetById(long id);
    Task<SearchResponse<Studio>> Search(SearchStudioRequest request, bool includeRelated = true);
}