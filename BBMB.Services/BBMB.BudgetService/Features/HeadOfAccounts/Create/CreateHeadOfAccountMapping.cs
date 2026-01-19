using BBMB.BudgetService.Features.HeadOfAccounts.Create.Dtos;
using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Create
{
    public class CreateHeadOfAccountMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateHeadOfAccountDto, HeadOfAccount>()
                .Map(dest => dest.ModificationDate, src => DateTime.UtcNow);
        }
    }
}
