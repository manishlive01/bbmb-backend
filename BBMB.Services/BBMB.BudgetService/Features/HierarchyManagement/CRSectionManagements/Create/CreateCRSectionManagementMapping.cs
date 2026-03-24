using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Create.Dtos;
using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Entites;
using Mapster;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Create
{
    public class CreateCRSectionManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateCRSectionManagementDto, CRSection>()
                .Map(dest => dest.CRId, src => src.CRId)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.Status, src => src.Status);
        }
    }
}