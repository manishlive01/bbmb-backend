using BBMB.BudgetService.Features.ObjectClassifications.Create.Dtos;
using BBMB.BudgetService.Features.ObjectClassifications.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.ObjectClassifications.Create
{
    public class CreateObjectClassificationMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateObjectClassificationDto, ObjectClassification>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
