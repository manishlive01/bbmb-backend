using BBMB.BudgetService.Features.EmployeeCharges.Create.Dtos;
using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.EmployeeCharges.Create
{
    public class CreateEmployeeChargeMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateEmployeeChargeDto, EmployeeCharge>()
                .Map(dest => dest.IsActive, src => true)
                .Map(dest => dest.IsChargeExpire, src => false);
        }
    }
}
