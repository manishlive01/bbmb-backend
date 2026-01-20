using BBMB.BudgetService.Features.Pools.Create.Dtos;
using BBMB.BudgetService.Features.Pools.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Pools.Create
{
    public class CreateRemittanceMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreatePoolDto, Pool>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
