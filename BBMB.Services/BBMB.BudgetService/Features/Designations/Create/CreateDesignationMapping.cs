using BBMB.BudgetService.Features.Designations.Create.Dtos;
using BBMB.BudgetService.Features.Designations.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Designations.Create
{
    public class CreateDesignationMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateDesignationDto, Designation>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
