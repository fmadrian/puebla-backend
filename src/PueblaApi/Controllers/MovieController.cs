using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PueblaApi.DTOS.Base;
using PueblaApi.DTOS.Movie;
using PueblaApi.Entities;
using PueblaApi.Helpers;
using PueblaApi.Repositories.Interfaces;
using PueblaApi.Settings;

namespace PueblaApi.Controllers;

[Route(ApiControllerRoutes.Movie)]
[ApiController]
public class MovieController : ControllerBase
{
    #region Dependencies and inject dependencies.
    // Inject dependencies
    private readonly ILogger<MovieController> _logger;
    private readonly IMapper _mapper;
    private readonly IMovieRepository _movieRepository;

    public MovieController(IMovieRepository movieRepository, ILogger<MovieController> logger, IMapper mapper)
    {
        this._movieRepository = movieRepository;
        this._mapper = mapper;
        this._logger = logger;
    }
    #endregion

    #region Endpoints

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(long id)
    {
        // 1. Search by ID.
        Movie movie = await this._movieRepository.GetById(id);
        if (movie == null)
            return NotFound(ResponseHelper.UnsuccessfulResponse(@"Movie {id} doesn't exist"));
        // 2. Return mapped response.
        return Ok(ResponseHelper.SuccessfulResponse(this._mapper.Map<MovieResponse>(movie)));
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchMovieRequest dto)
    {

        // 1. Take the response. retrieve the entities, map them into their respective response object.
        SearchResponse<Movie> result = await this._movieRepository.Search(dto);
        List<MovieResponse> items = result.Items.Select(this._mapper.Map<MovieResponse>).ToList();
        // 2. Return them into a search response object.
        return Ok(ResponseHelper.SuccessfulResponse(
            new SearchResponse<MovieResponse>(items, result.Page, result.PageSize, result.TotalCount)
        ));
    }



    #endregion
}
