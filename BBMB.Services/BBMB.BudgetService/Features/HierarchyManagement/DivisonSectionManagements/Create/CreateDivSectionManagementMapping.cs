using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Create.Dtos;
using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Create
{
    public class CreateDivSectionManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateDivSectionManagementDto, DivSection>()
                .Map(dest => dest.DivisionId, src => src.DivisionId)
                .Map(dest => dest.DivSectionCode, src => src.DivSectionCode)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.Status, src => src.Status);
        }
    }
}