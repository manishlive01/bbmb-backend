namespace BBMB.BudgetService.Features.BudgetProcess.Offices.SetWing.Dtos
{
    /// <summary>Body of <c>PUT /api/budget-offices/{id}/wing</c>: the corrected wing (irr_pwr code) of a CE office.</summary>
    public sealed class SetOfficeWingDto
    {
        public string WingCode { get; set; } = string.Empty;
    }
}
