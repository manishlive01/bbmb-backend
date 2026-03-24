using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Update.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Update
{
    public class UpdateDivSectionManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateDivSectionManagementDto, DivSection>()
                .Ignore(dest => dest.Id)
                .Map(dest => dest.DivisionId, src => src.DivisionId)
                .Map(dest => dest.DivSectionCode, src => src.DivSectionCode)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.Status, src => src.Status);
        }
    }
}