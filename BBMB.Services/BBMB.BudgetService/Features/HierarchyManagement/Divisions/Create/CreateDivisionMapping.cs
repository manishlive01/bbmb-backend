using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Create.Dtos;
using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.Create
{
    public class CreateDivisionMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateDivisionDto, Division>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
