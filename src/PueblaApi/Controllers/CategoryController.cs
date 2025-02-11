using System;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PueblaApi.DTOS.Base;
using PueblaApi.DTOS.Category;
using PueblaApi.Entities;
using PueblaApi.Exceptions;
using PueblaApi.Helpers;
using PueblaApi.Repositories.Interfaces;
using PueblaApi.RequestHelpers;
using PueblaApi.Settings;

namespace PueblaApi.Controllers;

[Route(ApiControllerRoutes.Category)]
[ApiController]
public class CategoryController : ControllerBase
{

    #region Dependencies and inject dependencies.
    // Inject dependencies
    private readonly ILogger<CategoryController> _logger;
    private readonly IMapper _mapper;
    private readonly ICategoryRepository _categoryRepository;

    public CategoryController(ICategoryRepository categoryRepository, ILogger<CategoryController> logger,
                            IMapper mapper)
    {
        this._categoryRepository = categoryRepository;
        this._mapper = mapper;
        this._logger = logger;

    }
    #endregion

    #region Endpoints

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin}, {ApiRoles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest dto)
    {
        try
        {
            // 1. Map DTO to entity.
            Category category = this._mapper.Map<Category>(dto);

            // 2. Store entity in database and return.
            category = await this._categoryRepository.Create(category);

            return CreatedAtAction(nameof(Get), new { Id = category.Id }, ResponseHelper.SuccessfulResponse<CategoryResponse>(
                new CategoryResponse()
                {
                    Id = category.Id,
                })
            );
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

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(long id)
    {
        try
        {
            // 1. Search by ID.
            Category category = await this._categoryRepository.GetById(id);
            if (category == null)
                return NotFound(ResponseHelper.UnsuccessfulResponse($"Category {id} doesn't exist"));
            // 2. Return mapped response.
            return Ok(ResponseHelper.SuccessfulResponse(this._mapper.Map<CategoryResponse>(category)));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchCategoryRequest dto)
    {
        try
        {
            // 1. Take the response. retrieve the entities, map them into their respective response object.
            SearchResponse<Category> result = await this._categoryRepository.Search(dto);
            List<CategoryResponse> items = result.Items.Select(this._mapper.Map<CategoryResponse>).ToList();
            // 2. Return them into a search response object.
            return Ok(ResponseHelper.SuccessfulResponse(
                new SearchResponse<CategoryResponse>(items, result.Page, result.PageSize, result.TotalCount)
            ));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin}, {ApiRoles.Manager}")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateCategoryRequest dto)
    {
        try
        {
            // 1. Search movie by ID.
            Category category = await this._categoryRepository.GetById(id);

            if (category == null)
                return NotFound($"Category {id} was not found.");

            // 2. Make changes from to DTO to entity.
            category.Name = dto.Name ?? category.Name;

            // 3. Store entity in database and return.
            category = await this._categoryRepository.Update(category);

            return Ok(ResponseHelper.SuccessfulResponse("Updated."));
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

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin}, {ApiRoles.Manager}")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            // 1. Map DTO to entity.
            Category category = await this._categoryRepository.GetById(id);

            if (category == null)
                return NotFound($"Studio {id} was not found.");

            // 2. Remove entity from database and return.
            await this._categoryRepository.Delete(category);

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
