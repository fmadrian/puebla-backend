using AutoMapper;
using PueblaApi.DTOS.Category;
using PueblaApi.DTOS.Movie;
using PueblaApi.DTOS.Studio;
using PueblaApi.Entities;

namespace PueblaApi.Mapping;

public partial class MappingProfiles : Profile
{
    /**
           *   When we mapping list into another list using MapFrom ((source, target, result, context))
           *       Source: The object we have to map.
           *       Target: The result we want.
           *       Result: The list of mapped objects we get after mapping all the entities.
           */
    public MappingProfiles()
    {
        #region Movies
        CreateMap<Movie, MovieResponse>()
        .ForMember(
            destination => destination.Studio,
            action => action.MapFrom(
                (source, target, result, context) => context.Mapper.Map<StudioResponse>(source.Studio)
            )
        ).ForMember(
            destination => destination.Categories,
            action => action.MapFrom(
                (source, target, result, context) => context.Mapper.Map<List<CategoryResponse>>(source.Categories)
            )
        );

        CreateMap<CreateMovieRequest, Movie>()
        .ForMember(
            destination => destination.ImageURL,
            action => action.Ignore()
        ).ForMember(
            destination => destination.Studio,
            action => action.MapFrom(
                (source, target, result, context) => context.Mapper.Map<Studio>(source.Studio)
            )
        ).ForMember(
            destination => destination.Categories,
            action => action.MapFrom(
                (source, target, result, context) => context.Mapper.Map<List<Category>>(source.Categories)
            )
        );

        #endregion

        #region Categories
        CreateMap<Category, CategoryResponse>();
        CreateMap<long, Category>().ForMember(
            destination => destination.Id,
            action => action.MapFrom(
                (source, target, result, context) => source
            )
        );
        #endregion

        #region Studios
        CreateMap<CreateStudioRequest, Studio>();
        CreateMap<Studio, StudioResponse>();
        CreateMap<long, Studio>().ForMember(
            destination => destination.Id,
            action => action.MapFrom(
                (source, target, result, context) => source
            )
        );
        #endregion
    }
}
