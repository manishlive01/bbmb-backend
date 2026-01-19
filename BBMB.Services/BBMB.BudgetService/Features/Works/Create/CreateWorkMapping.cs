using BBMB.BudgetService.Features.Works.Create.Dtos;
using BBMB.BudgetService.Features.Works.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Works.Create
{
    public class CreateWorkMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateWorkDto, Work>();
        }
    }
}
