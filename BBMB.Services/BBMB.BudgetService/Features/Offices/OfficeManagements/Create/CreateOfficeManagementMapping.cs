using BBMB.BudgetService.Features.Offices.OfficeManagements.Create.Dtos;
using BBMB.BudgetService.Features.Offices.OfficeManagements.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Create
{
    public class CreateOfficeManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateOfficeManagementDto, OfficeManagement>();
        }
    }
}