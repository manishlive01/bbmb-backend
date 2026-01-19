using BBMB.BudgetService.Features.Regions.Entities;
using BBMB.BudgetService.Features.Regions.Update.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.Regions.Update
{
    public class UpdateRegionMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateRegionDto, Region>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
