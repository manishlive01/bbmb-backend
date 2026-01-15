using System.ComponentModel.DataAnnotations;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Create.Dtos
{
    public class CreateMajorHeadDto
    {
        public string MajorAccCode { get; set; } = string.Empty;
        public string MajorAccDesc { get; set; } = string.Empty;
    }
}
