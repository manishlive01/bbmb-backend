using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Assign
{
    /// <summary>An office and at least one code, each id positive.</summary>
    public sealed class AssignHeadsValidator : AbstractValidator<AssignHeadsCommand>
    {
        public AssignHeadsValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.OfficeId).GreaterThan(0);
                RuleFor(x => x.Dto.AccountCodeIds).NotEmpty();
                RuleForEach(x => x.Dto.AccountCodeIds).GreaterThan(0);
            });
        }
    }
}
