using BBMB.BudgetService.Features.CEManagements.Entities;
using BBMB.BudgetService.Features.CEManagements.Update.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.CEManagements.Update
{
    public class UpdateCEManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateCEManagementDto, CEManagement>();
        }
    }
}
