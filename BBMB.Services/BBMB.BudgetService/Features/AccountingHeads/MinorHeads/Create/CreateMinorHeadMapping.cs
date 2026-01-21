using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create.Dtos;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create
{
    public class CreateMinorHeadMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateMinorHeadDto, MinorHead>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
