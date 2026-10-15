using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals
{
    /// <summary>
    /// Design §8.2: categories non-empty, each WORKS or DNA in P1 (RECEIPTS and CAPEX arrive in P3, when F-15
    /// and F-16 rules exist); officeIds positive.
    /// </summary>
    public sealed class OpenProposalsValidator : AbstractValidator<OpenProposalsCommand>
    {
        /// <summary>Categories that may be opened in the deployed phase.</summary>
        public static readonly IReadOnlyList<string> OpenableCategories = [ProposalCategory.Works, ProposalCategory.Dna];

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
