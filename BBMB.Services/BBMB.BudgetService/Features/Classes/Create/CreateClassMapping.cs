using BBMB.BudgetService.Features.Classes.Create.Dtos;
using BBMB.BudgetService.Features.Classes.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Classes.Create
{
    public class CreateClassMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateClassDto, Class>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
