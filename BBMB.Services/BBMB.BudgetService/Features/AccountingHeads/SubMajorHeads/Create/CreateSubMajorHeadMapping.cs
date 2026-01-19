using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create.Dtos;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create
{
    public class CreateSubMajorHeadMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateSubMajorHeadDto, SubMajorHead>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
