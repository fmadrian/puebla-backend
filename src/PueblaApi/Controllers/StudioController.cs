using System;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PueblaApi.DTOS.Base;
using PueblaApi.DTOS.Studio;
using PueblaApi.Entities;
using PueblaApi.Exceptions;
using PueblaApi.Helpers;
using PueblaApi.Repositories.Interfaces;
using PueblaApi.RequestHelpers;
using PueblaApi.Settings;

namespace PueblaApi.Controllers;

[Route(ApiControllerRoutes.Studio)]
[ApiController]
public class StudioController : ControllerBase
{
    #region Dependencies and inject dependencies.
    // Inject dependencies
    private readonly ILogger<StudioController> _logger;
    private readonly IMapper _mapper;
    private readonly IStudioRepository _studioRepository;

    public StudioController(IStudioRepository studioRepository, ILogger<StudioController> logger,
                            IMapper mapper)
    {
        this._studioRepository = studioRepository;
        this._mapper = mapper;
        this._logger = logger;

    }
    #endregion

    #region Endpoints

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(long id)
    {
        try
        {
            // 1. Search by ID.
            Studio studio = await this._studioRepository.GetById(id);
            if (studio == null)
                return NotFound(ResponseHelper.UnsuccessfulResponse(@"Studio {id} doesn't exist"));
            // 2. Return mapped response.
            return Ok(ResponseHelper.SuccessfulResponse(this._mapper.Map<StudioResponse>(studio)));
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
            Studio studio = await this._studioRepository.GetById(id);

            if (studio == null)
                return NotFound($"Studio {id} was not found.");

            // 2. Remove entity from database and return.
            await this._studioRepository.Delete(studio);

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

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchStudioRequest dto)
    {
        try
        {
            // 1. Take the response. retrieve the entities, map them into their respective response object.
            SearchResponse<Studio> result = await this._studioRepository.Search(dto);
            List<StudioResponse> items = result.Items.Select(this._mapper.Map<StudioResponse>).ToList();
            // 2. Return them into a search response object.
            return Ok(ResponseHelper.SuccessfulResponse(
                new SearchResponse<StudioResponse>(items, result.Page, result.PageSize, result.TotalCount)
            ));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }


    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin}, {ApiRoles.Manager}")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateStudioRequest dto)
    {
        try
        {
            // 1. Search movie by ID.
            Studio studio = await this._studioRepository.GetById(id);

            if (studio == null)
                return NotFound($"Studio {id} was not found.");

            // 2. Make changes from to DTO to entity.
            studio.Name = dto.Name ?? studio.Name;
            studio.Country = dto.Country ?? studio.Country;
            studio.FoundationYear = dto.FoundationYear ?? studio.FoundationYear;


            // 4. Store entity in database and return.
            studio = await this._studioRepository.Create(studio);

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

    #endregion

}
