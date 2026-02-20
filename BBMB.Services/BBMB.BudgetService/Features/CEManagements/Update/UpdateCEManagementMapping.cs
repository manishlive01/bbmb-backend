using BBMB.BudgetService.Features.CEManagements.Create.Dtos;
using BBMB.BudgetService.Features.CEManagements.Entities;
using BBMB.BudgetService.Features.CEManagements.Update.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.CEManagements.Update
{
    public class UpdateCEManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateCEManagementDto, CEManagement>()
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
