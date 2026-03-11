namespace BBMB.BudgetService.Features.EmployeeCharges.GetListByUser.Dtos
{
    public class GetEmployeeChargeByUserResponseDto
    {

        public int ProjectId { get; set; }

        public string CECode { get; set; } = null!;

        public string CRCode { get; set; } = null!;

        public string DivCode { get; set; } = null!;
    }
}