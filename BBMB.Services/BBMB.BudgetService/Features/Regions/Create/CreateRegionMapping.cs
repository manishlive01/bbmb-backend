using BBMB.BudgetService.Features.Regions.Create.Dtos;
using BBMB.BudgetService.Features.Regions.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Regions.Create
{
    public static class CreateRegionMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<CreateRegionDto, Region>
                .NewConfig()

                // ===== Simple direct mappings =====
                .Map(dest => dest.RegionCode,
                     src => src.RegionCode.Trim().ToUpper())

                .Map(dest => dest.RegionDesc,
                     src => src.RegionDesc.Trim())

                .Map(dest => dest.UserCode,
                     src => src.UserCode.Trim())

                // ===== Ignore DB / system-controlled fields =====
                .Ignore(dest => dest.ModificationDate);
        }
    }
}
