using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Update.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Update
{
    public class UpdateCRManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateCRManagementDto, CRManagement>()
                //.Map(dest => dest.CECode, src => src.CECode)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.Status, src => src.Status)

                .Ignore(dest => dest.Date)
                .Ignore(dest => dest.UserCode)
                .Ignore(dest => dest.CEManagement);
        }
    }
}
