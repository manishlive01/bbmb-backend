using BBMB.BudgetService.Features.Projects.Create.Dtos;
using BBMB.BudgetService.Features.Projects.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Projects.Create
{
    public class CreateProjectMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateProjectDto, Project>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
