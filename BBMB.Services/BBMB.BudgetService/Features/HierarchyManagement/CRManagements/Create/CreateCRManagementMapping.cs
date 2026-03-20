using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Create.Dtos;
using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Create
{
    public class CreateCRManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateCRManagementDto, CRManagement>()
                .Map(dest => dest.CEId, src => src.CEId)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)

                // 🔒 Backend controlled fields (never from client)
                .Ignore(dest => dest.Date)
                .Ignore(dest => dest.UserCode)


                // 🔒 Navigation property must not be mapped
                .Ignore(dest => dest.CEManagement);
        }
    }
}
