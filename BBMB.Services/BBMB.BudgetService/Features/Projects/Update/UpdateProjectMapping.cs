using BBMB.BudgetService.Features.Projects.Entities;
using BBMB.BudgetService.Features.Projects.Update.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.Projects.Update
{
    public class UpdateProjectMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateProjectDto, Project>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
