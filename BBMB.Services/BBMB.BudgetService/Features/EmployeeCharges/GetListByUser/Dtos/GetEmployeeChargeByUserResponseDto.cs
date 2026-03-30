using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.EmployeeCharges.GetListByUser.Dtos
{
    public class GetEmployeeChargeByUserResponseDto
    {

        public string ProjectCode { get; set; }
        public string ProjDesc { get; set; } = null!;

        public int? CEId { get; set; }
        public int? CECode { get; set; }
        public string? CEName { get; set; } = null!;
        public int? CRId { get; set; }
        public int? CRCode { get; set; }
        public string? CRName { get; set; } = null!;
        public int? DivisionId { get; set; }
        public int? DivisionCode { get; set; }
        public string? DivName { get; set; } = null!;
        public bool IsChargeExpire { get; set; }
    }
}