using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.EmployeeCharges.GetListByUser.Dtos
{
    public class GetEmployeeChargeByUserResponseDto
    {

        public string ProjectCode { get; set; }
        public string ProjDesc { get; set; } = null!;

        public string CECode { get; set; } = null!;
        public string CEName { get; set; } = null!;

        public string CRCode { get; set; } = null!;
        public string CRName { get; set; } = null!;
        public int DivisionCode { get; set; }
        public string DivName { get; set; } = null!;
    }
}