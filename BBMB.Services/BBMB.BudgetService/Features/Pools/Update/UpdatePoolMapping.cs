using BBMB.BudgetService.Features.Pools.Entities;
using BBMB.BudgetService.Features.Pools.Update.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.Pools.Update
{
    public class UpdatePoolMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdatePoolDto, Pool>()
                .Map(dest => dest.PoolDesc, src => src.PoolDesc)
                .Map(dest => dest.UserCode, src => src.UserCode)
                .Ignore(dest => dest.PoolCode)
                .Ignore(dest => dest.ModificationDate);
        }
    }
}
