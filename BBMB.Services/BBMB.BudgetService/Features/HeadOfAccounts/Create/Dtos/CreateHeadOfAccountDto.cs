namespace BBMB.BudgetService.Features.HeadOfAccounts.Create.Dtos
{
    /// <summary>
    /// F-01 body (design §8.4; BUD-039 to BUD-046, BUD-322). At each of the six levels a code is either selected
    /// (it exists under its parent: no description is sent, the Manual's nomenclature is shown) or entered new (its
    /// description is required). The complete code, origin, creation particulars and the ACC-009 flag are set by the
    /// module and accept no entry (BUD-042, BUD-267).
    /// </summary>
    public sealed class CreateHeadOfAccountDto
    {
        public string MajorAccCode { get; set; } = string.Empty;
        public string? MajorAccDesc { get; set; }
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string? SubMajorAccDesc { get; set; }
        public string MinorAccCode { get; set; } = string.Empty;
        public string? MinorAccDesc { get; set; }
        public string SubMinorAccCode { get; set; } = string.Empty;
        public string? SubMinorAccDesc { get; set; }
        public string DetailAccCode { get; set; } = string.Empty;
        public string? DetailAccDesc { get; set; }
        public string ObjectAccCode { get; set; } = string.Empty;
        public string? ObjectAccDesc { get; set; }
        /// <summary>Description of the complete head, ≤ 150.</summary>
        public string HeadDesc { get; set; } = string.Empty;
        /// <summary>The e-Office reference of the Group Head's approval (BUD-044, M-03), ≤ 50.</summary>
        public string EofficeApprovalRef { get; set; } = string.Empty;
    }
}
