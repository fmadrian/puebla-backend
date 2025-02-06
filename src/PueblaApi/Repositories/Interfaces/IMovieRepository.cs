using PueblaApi.DTOS.Base;
using PueblaApi.DTOS.Movie;
using PueblaApi.Entities;

namespace PueblaApi.Repositories.Interfaces;

public interface IMovieRepository : IRepository<Movie>
{
    Task<Movie?> GetById(long id);
    Task<SearchResponse<Movie>> Search(SearchMovieRequest request, bool includeRelated = true);
}
