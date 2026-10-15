using BBMB.BudgetService.Shared.Messages;
using FluentValidation;
namespace BBMB.BudgetService.Features.HeadOfAccounts.Create
{
    /// <summary>
    /// F-01 shape (design §8.4, §7 VR-01): segment patterns 4-2-3-2-3-3 digits (DDD T-01 to T-06 CHECKs, BUD-041);
    /// e-Office reference required ≤ 50 (M-03, BUD-044); segment descriptions ≤ 200 as entered (AN(200); the object
    /// column is wider only for the legacy load); head description required ≤ 150. Typed to the command (D-3), so it runs.
    /// </summary>
    public sealed class CreateHeadOfAccountValidator : AbstractValidator<CreateHeadOfAccountCommand>
    {
        public CreateHeadOfAccountValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.MajorAccCode).Matches(@"^\d{4}$").WithErrorCode(BudgetMessages.M22).WithMessage("majorAccCode: 4 digits.");
                RuleFor(x => x.Dto.SubMajorAccCode).Matches(@"^\d{2}$").WithErrorCode(BudgetMessages.M22).WithMessage("subMajorAccCode: 2 digits.");
                RuleFor(x => x.Dto.MinorAccCode).Matches(@"^\d{3}$").WithErrorCode(BudgetMessages.M22).WithMessage("minorAccCode: 3 digits.");
                RuleFor(x => x.Dto.SubMinorAccCode).Matches(@"^\d{2}$").WithErrorCode(BudgetMessages.M22).WithMessage("subMinorAccCode: 2 digits.");
                RuleFor(x => x.Dto.DetailAccCode).Matches(@"^\d{3}$").WithErrorCode(BudgetMessages.M22).WithMessage("detailAccCode: 3 digits.");
                RuleFor(x => x.Dto.ObjectAccCode).Matches(@"^\d{3}$").WithErrorCode(BudgetMessages.M22).WithMessage("objectAccCode: 3 digits.");
                RuleFor(x => x.Dto.MajorAccDesc).MaximumLength(200);
                RuleFor(x => x.Dto.SubMajorAccDesc).MaximumLength(200);
                RuleFor(x => x.Dto.MinorAccDesc).MaximumLength(200);
                RuleFor(x => x.Dto.SubMinorAccDesc).MaximumLength(200);
                RuleFor(x => x.Dto.DetailAccDesc).MaximumLength(200);
                RuleFor(x => x.Dto.ObjectAccDesc).MaximumLength(200);
                RuleFor(x => x.Dto.HeadDesc).Must(t => !string.IsNullOrWhiteSpace(t)).WithErrorCode(BudgetMessages.M21)
                    .WithMessage(BudgetMessages.Format(BudgetMessages.M21)).MaximumLength(150);
                RuleFor(x => x.Dto.EofficeApprovalRef).Must(t => !string.IsNullOrWhiteSpace(t)).WithErrorCode(BudgetMessages.M03)
                    .WithMessage(BudgetMessages.Format(BudgetMessages.M03)).MaximumLength(50);
            });
        }
    }
}
