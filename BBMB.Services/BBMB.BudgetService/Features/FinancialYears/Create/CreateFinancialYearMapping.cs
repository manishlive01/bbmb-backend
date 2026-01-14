using BBMB.BudgetService.Features.FinancialYears.Create.Dtos;
using BBMB.BudgetService.Features.FinancialYears.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.FinancialYears.Create
{
    public class CreateFinancialYearMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateFinancialYearDto, FinancialYear>()
                .Map(dest => dest.FinYearClosing, src => src.FinYearClosing)
                .Map(dest => dest.StartDate, src => src.StartDate)
                .Map(dest => dest.EndDate, src => src.EndDate)
                .Map(dest => dest.Closed, src => src.Closed);
        }
    }
}
