using BBMB.BudgetService.Features.HeadOfAccounts.Create.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Create
{
    public class CreateHeadOfAccountValidator : AbstractValidator<CreateHeadOfAccountDto>
    {
        private readonly BudgetContext _context;

        public CreateHeadOfAccountValidator(BudgetContext context)
        {
            _context = context;

            RuleFor(x => x.MajorAccCode)
                .NotEmpty().WithMessage("Major head code is required")
                .MaximumLength(4).WithMessage("Major head code must not exceed 4 characters")
                .MustAsync(MajorHeadExists).WithMessage("Major head code does not exist");

            RuleFor(x => x.SubMajorAccCode)
                .MaximumLength(4).WithMessage("Sub-major head code must not exceed 4 characters")
                .MustAsync(SubMajorHeadExistsIfProvided).WithMessage("Sub-major head code does not exist")
                .When(x => !string.IsNullOrEmpty(x.SubMajorAccCode));

            RuleFor(x => x.MinorAccCode)
                .NotEmpty().WithMessage("Minor head code is required")
                .MaximumLength(3).WithMessage("Minor head code must not exceed 3 characters")
                .MustAsync(MinorHeadExists).WithMessage("Minor head code does not exist");

            RuleFor(x => x.SubMinorAccCode)
                .MaximumLength(2).WithMessage("Sub-minor head code must not exceed 2 characters")
                .MustAsync(SubMinorHeadExistsIfProvided).WithMessage("Sub-minor head code does not exist")
                .When(x => !string.IsNullOrEmpty(x.SubMinorAccCode));

            RuleFor(x => x.DetailAccCode)
                .NotEmpty().WithMessage("Detail head code is required")
                .MaximumLength(3).WithMessage("Detail head code must not exceed 3 characters")
                .MustAsync(DetailHeadExists).WithMessage("Detail head code does not exist");

            RuleFor(x => x.ObjectAccCode)
                .NotEmpty().WithMessage("Object classification code is required")
                .MaximumLength(3).WithMessage("Object classification code must not exceed 3 characters")
                .MustAsync(ObjectClassificationExists).WithMessage("Object classification code does not exist");

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

        private async Task<bool> MajorHeadExists(string code, CancellationToken ct)
        {
            return await _context.MajorHeads.AnyAsync(m => m.MajorAccCode == code, ct);
        }

        private async Task<bool> SubMajorHeadExistsIfProvided(CreateHeadOfAccountDto dto, string? code, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(code)) return true;
            return await _context.SubMajorHeads.AnyAsync(s => s.SubMajorAccCode == code, ct);
        }

        private async Task<bool> MinorHeadExists(string code, CancellationToken ct)
        {
            return await _context.MinorHeads.AnyAsync(m => m.MinorAccCode == code, ct);
        }

        private async Task<bool> SubMinorHeadExistsIfProvided(CreateHeadOfAccountDto dto, string? code, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(code)) return true;
            return await _context.SubMinorHeads.AnyAsync(s => s.SubMinorAccCode == code, ct);
        }

        private async Task<bool> DetailHeadExists(string code, CancellationToken ct)
        {
            return await _context.DetailHeads.AnyAsync(d => d.DetailAccCode == code, ct);
        }

        private async Task<bool> ObjectClassificationExists(string code, CancellationToken ct)
        {
            return await _context.ObjectClassifications.AnyAsync(o => o.ObjectAccCode == code, ct);
        }
    }
}
