using BBMB.BudgetService.Features.Offices.RemittanceOffices.Create.Dtos;
using BBMB.BudgetService.Features.Offices.RemittanceOffices.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.Create
{
    public class CreateRemittanceMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateRemittanceDto, RemittanceOffice>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
