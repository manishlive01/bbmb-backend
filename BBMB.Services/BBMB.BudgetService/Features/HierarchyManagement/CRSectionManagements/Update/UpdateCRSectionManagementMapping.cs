using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Entites;
using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Update.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Update
{
    public class UpdateCRSectionManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateCRSectionManagementDto, CRSection>()
                .Ignore(dest => dest.Id) // ⚠️ important
                .Map(dest => dest.CRId, src => src.CRId)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.Status, src => src.Status);
        }
    }
}