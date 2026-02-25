using BBMB.BudgetService.Features.CEManagements.Create.Dtos;
using BBMB.BudgetService.Features.CEManagements.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.CEManagements.Create
{
    public class CreateCEManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateCEManagementDto, CEManagement>()
    .Map(dest => dest.CECode, src => src.CECode)
    .Map(dest => dest.Name, src => src.Name)
    .Map(dest => dest.ProjectCode, src => src.ProjectCode)
    .Map(dest => dest.Description, src => src.Description)
    .Map(dest => dest.Location, src => src.Location)
    .Map(dest => dest.Date, src => src.Date)
    .Map(dest => dest.Status, src => src.Status)
    .Map(dest => dest.UserCode, src => src.UserCode);

        }
    }
}
