using System.ComponentModel.DataAnnotations;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetList.Dtos
{
    public class GetMajorHeadListResponseDto
    {
        public string MajorAccCode { get; set; } = string.Empty;
        public string MajorAccDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
