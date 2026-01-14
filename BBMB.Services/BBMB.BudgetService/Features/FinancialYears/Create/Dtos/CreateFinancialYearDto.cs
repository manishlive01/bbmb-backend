using System.ComponentModel.DataAnnotations;

namespace BBMB.BudgetService.Features.FinancialYears.Create.Dtos
{
    public class CreateFinancialYearDto
    {
        public int FinYearClosing { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime? Closed { get; set; }
    }
}
