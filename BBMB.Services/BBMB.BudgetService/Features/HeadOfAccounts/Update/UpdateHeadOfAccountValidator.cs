using BBMB.BudgetService.Features.HeadOfAccounts.Update.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Update
{
    public class UpdateHeadOfAccountValidator : AbstractValidator<UpdateHeadOfAccountDto>
    {
        public UpdateHeadOfAccountValidator()
        {
            RuleFor(x => x.HeadDesc)
                .NotEmpty().WithMessage("Head description is required")
                .MaximumLength(150).WithMessage("Head description must not exceed 150 characters");

            RuleFor(x => x.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");

            RuleFor(x => x.PartyEmpRequired)
                .MaximumLength(1).WithMessage("Party employee required must not exceed 1 character")
                .When(x => !string.IsNullOrEmpty(x.PartyEmpRequired));

            RuleFor(x => x.PartyLedgToPrint)
                .MaximumLength(1).WithMessage("Party ledger to print must not exceed 1 character")
                .When(x => !string.IsNullOrEmpty(x.PartyLedgToPrint));

            RuleFor(x => x.TdsDeducted)
                .MaximumLength(1).WithMessage("TDS deducted must not exceed 1 character")
                .When(x => !string.IsNullOrEmpty(x.TdsDeducted));

            RuleFor(x => x.AdvanceHead)
                .MaximumLength(1).WithMessage("Advance head must not exceed 1 character")
                .When(x => !string.IsNullOrEmpty(x.AdvanceHead));

            RuleFor(x => x.BankCash)
                .MaximumLength(1).WithMessage("Bank cash must not exceed 1 character")
                .When(x => !string.IsNullOrEmpty(x.BankCash));

            RuleFor(x => x.MonthlyCarryFwd)
                .MaximumLength(1).WithMessage("Monthly carry forward must not exceed 1 character")
                .When(x => !string.IsNullOrEmpty(x.MonthlyCarryFwd));
        }
    }
}
