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
                // The request keeps DateTime (no API change); the column is a business date (T-08).
                .Map(dest => dest.StartDate, src => DateOnly.FromDateTime(src.StartDate))
                .Map(dest => dest.EndDate, src => DateOnly.FromDateTime(src.EndDate))
                .Map(dest => dest.Closed, src => src.Closed);
        }
    }
}
