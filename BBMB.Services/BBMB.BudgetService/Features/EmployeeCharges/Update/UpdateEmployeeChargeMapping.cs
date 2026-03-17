using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using BBMB.BudgetService.Features.EmployeeCharges.Update.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.EmployeeCharges.Update
{
    public class UpdateEmployeeChargeMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateEmployeeChargeDto, EmployeeCharge>();
        }
    }
}
