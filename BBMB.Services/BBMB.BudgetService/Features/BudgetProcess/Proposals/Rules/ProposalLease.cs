using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using Microsoft.Extensions.Configuration;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules
{
    /// <summary>
    /// Design §4.10 edit lease (BUD-015, M-23). Only an officer who could write the proposal in its current state
    /// may take it; the write right of each state is <see cref="WriteRightFor"/>. The length is
    /// <c>BudgetProcess:EditLeaseMinutes</c>, design default 20 (OI-14).
    /// </summary>
    public static class ProposalLease
    {
        public const string MinutesKey = "BudgetProcess:EditLeaseMinutes";
        public const int DefaultMinutes = 20;

        /// <summary>The right that writes the proposal in <paramref name="state"/>, or null where no state write exists.</summary>
        public static string? WriteRightFor(string state, string category) => state switch
        {
            ProposalStateCode.S01 or ProposalStateCode.S04 => category == ProposalCategory.Works ? "F07.Enter" : "F11.Enter",
            ProposalStateCode.S03 or ProposalStateCode.S06 or ProposalStateCode.S08 => "Review.Examine",
            ProposalStateCode.S10 => category == ProposalCategory.Works ? "F10.Act" : "F13.Act",
            _ => null,
        };

        public static TimeSpan Length(IConfiguration configuration) =>
            TimeSpan.FromMinutes(configuration.GetValue<int?>(MinutesKey) is { } m && m > 0 ? m : DefaultMinutes);

        public static bool IsLive(BudgetProposal p, DateTimeOffset now) => p.EditLeaseBy is not null && p.EditLeaseExpiresAt > now;

        /// <summary>True when another officer holds a live lease: that officer's write is refused with M-23.</summary>
        public static bool IsHeldByOther(BudgetProposal p, Guid employeeId, DateTimeOffset now) =>
            IsLive(p, now) && p.EditLeaseBy != employeeId;
    }
}
