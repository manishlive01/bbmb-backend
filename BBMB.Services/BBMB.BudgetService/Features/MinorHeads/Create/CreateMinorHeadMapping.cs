using BBMB.BudgetService.Features.MinorHeads.Create.Dtos;
using BBMB.BudgetService.Features.MinorHeads.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.MinorHeads.Create
{
    public class CreateMinorHeadMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateMinorHeadDto, MinorHead>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
