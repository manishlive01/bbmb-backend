using BBMB.BudgetService.Features.Divisions.Create.Dtos;
using BBMB.BudgetService.Features.Divisions.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Divisions.Create
{
    public class CreateDivisionMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateDivisionDto, Division>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
