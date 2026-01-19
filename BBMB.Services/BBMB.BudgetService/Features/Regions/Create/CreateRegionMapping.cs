using BBMB.BudgetService.Features.Regions.Create.Dtos;
using BBMB.BudgetService.Features.Regions.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Regions.Create
{
    public class CreateRegionMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateRegionDto, Region>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
