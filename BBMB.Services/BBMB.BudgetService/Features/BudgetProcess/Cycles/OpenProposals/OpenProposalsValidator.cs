using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals
{
    /// <summary>
    /// Design §8.2: categories non-empty, each a category the deployed phase can open; officeIds positive.
    /// P3 enables RECEIPTS (F-15) and CAPEX (F-16) alongside WORKS and DNA, now that their only added rule
    /// exists (VR-15 is not applied to RECEIPTS, BUD-318; the expenditure nature follows the category).
    /// </summary>
    public sealed class OpenProposalsValidator : AbstractValidator<OpenProposalsCommand>
    {
        /// <summary>Categories that may be opened in the deployed phase (all four from P3).</summary>
        public static readonly IReadOnlyList<string> OpenableCategories =
            [ProposalCategory.Works, ProposalCategory.Dna, ProposalCategory.Receipts, ProposalCategory.Capex];

        public OpenProposalsValidator()
        {
            RuleFor(x => x.CycleId).GreaterThan(0);
            RuleFor(x => x.Dto.Categories).NotEmpty().WithMessage("At least one category is required.");
            RuleForEach(x => x.Dto.Categories)
                .Must(c => OpenableCategories.Contains(c))
                .WithMessage(c => $"Category must be one of {string.Join(", ", OpenableCategories)}.");
            RuleForEach(x => x.Dto.OfficeIds).GreaterThan(0).WithMessage("officeIds must be positive.");
        }
    }
}
