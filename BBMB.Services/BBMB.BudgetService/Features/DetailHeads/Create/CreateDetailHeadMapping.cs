using BBMB.BudgetService.Features.DetailHeads.Create.Dtos;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.DetailHeads.Create
{
    public class CreateDetailHeadMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateDetailHeadDto, DetailHead>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
