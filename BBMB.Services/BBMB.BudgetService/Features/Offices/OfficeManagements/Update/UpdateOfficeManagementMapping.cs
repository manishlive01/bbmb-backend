using BBMB.BudgetService.Features.Offices.OfficeManagements.Update.Dtos;
using BBMB.BudgetService.Features.Offices.OfficeManagements.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Update
{
    public class UpdateOfficeManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateOfficeManagementDto, OfficeManagement>();
        }
    }
}