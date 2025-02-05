using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PueblaApi.DTOS.Base;
using PueblaApi.DTOS.Movie;
using PueblaApi.Entities;
using PueblaApi.Exceptions;
using PueblaApi.Helpers;
using PueblaApi.Repositories.Interfaces;
using PueblaApi.Services.Interfaces;
using PueblaApi.Settings;
using PueblaApi.RequestHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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
    private readonly IImageService _imageService;

    public MovieController(IMovieRepository movieRepository, ILogger<MovieController> logger,
                            IMapper mapper, IImageService imageService)
    {
        this._movieRepository = movieRepository;
        this._mapper = mapper;
        this._logger = logger;
        this._imageService = imageService;
    }
    #endregion

    #region Endpoints

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(long id)
    {
        try
        {
            // 1. Search by ID.
            Movie movie = await this._movieRepository.GetById(id);
            if (movie == null)
                return NotFound(ResponseHelper.UnsuccessfulResponse(@"Movie {id} doesn't exist"));
            // 2. Return mapped response.
            return Ok(ResponseHelper.SuccessfulResponse(this._mapper.Map<MovieResponse>(movie)));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchMovieRequest dto)
    {
        try
        {
            // 1. Take the response. retrieve the entities, map them into their respective response object.
            SearchResponse<Movie> result = await this._movieRepository.Search(dto);
            List<MovieResponse> items = result.Items.Select(this._mapper.Map<MovieResponse>).ToList();
            // 2. Return them into a search response object.
            return Ok(ResponseHelper.SuccessfulResponse(
                new SearchResponse<MovieResponse>(items, result.Page, result.PageSize, result.TotalCount)
            ));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin}, {ApiRoles.Manager}")]
    public async Task<IActionResult> Create([FromForm] CreateMovieRequest dto)
    {
        string imagePublicID = "";
        try
        {
            // 1. Map DTO to entity.
            Movie movie = this._mapper.Map<Movie>(dto);

            // 2. Upload image and get public ID to add it to entity.
            if (dto.Image != null)
            {
                imagePublicID = await this._imageService.UploadImage(dto.Image, null);
                movie.ImageURL = imagePublicID;
            }
            // 3. Store entity in database and return.
            movie = await this._movieRepository.Create(movie);

            return CreatedAtAction(nameof(Get), new { Id = movie.Id }, ResponseHelper.SuccessfulResponse<MovieResponse>(
                new MovieResponse()
                {
                    Id = movie.Id,
                    ImageURL = movie.ImageURL
                })
            );
        }
        catch (ApiException e)
        {
            // Delete image if there is an exception after saving an image.
            if (imagePublicID != "")
                await this._imageService.DeleteImage(imagePublicID);

            return BadRequest(ResponseHelper.UnsuccessfulResponse(e.Message));
        }
        catch (Exception e)
        {
            // Delete image if there is an exception after saving an image.
            if (imagePublicID != "")
                await this._imageService.DeleteImage(imagePublicID);
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin}, {ApiRoles.Manager}")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            // 1. Map DTO to entity.
            Movie movie = await this._movieRepository.GetById(id);

            if (movie == null)
                return NotFound($"Movie {id} was not found.");

            // 2. Delete image before deleting the object.
            if (movie.ImageURL != null)
                await this._imageService.DeleteImage(movie.ImageURL);

            // 3. Remove entity from database and return.
            await this._movieRepository.Delete(movie);

            return Ok(ResponseHelper.SuccessfulResponse("Deleted."));
        }
        catch (ApiException e)
        {
            return BadRequest(ResponseHelper.UnsuccessfulResponse(e.Message));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }




    #endregion
}
