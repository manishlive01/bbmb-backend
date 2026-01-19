using BBMB.BudgetService.Features.PowerIrrigations.Create.Dtos;
using BBMB.BudgetService.Features.PowerIrrigations.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.PowerIrrigations.Create
{
    public class CreatePowerIrrigationMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreatePowerIrrigationDto, PowerIrrigation>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
