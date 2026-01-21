using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Update.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.Update
{
    public sealed class UpdateSubMinorHeadValidator : AbstractValidator<UpdateSubMinorHeadCommand>
    {
        private readonly BudgetContext _context;

        public UpdateSubMinorHeadValidator(BudgetContext context)
        {
            _context = context;

            RuleFor(x => x.dto.SubMinorAccDesc)
                .NotEmpty().WithMessage("Sub-minor account description is required")
                .MaximumLength(100).WithMessage("Sub-minor account description must not exceed 100 characters");

            //RuleFor(x => x.MajorAccCode)
            //    .NotEmpty().WithMessage("Major account code is required")
            //    .MaximumLength(4).WithMessage("Major account code must not exceed 4 characters")
            //    .MustAsync(MajorHeadExists).WithMessage("Major account code does not exist");

            //RuleFor(x => x.SubMajorAccCode)
            //    .NotEmpty().WithMessage("Sub-major account code is required")
            //    .MaximumLength(2).WithMessage("Sub-major account code must not exceed 2 characters")
            //    .MustAsync(SubMajorHeadExists).WithMessage("Sub-major account code does not exist");

            RuleFor(x => x.dto.MinorAccCode)
                .NotEmpty().WithMessage("Minor account code is required")
                .MaximumLength(3).WithMessage("Minor account code must not exceed 3 characters")
                .MustAsync(MinorHeadExists).WithMessage("Minor account code does not exist");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }

        private async Task<bool> MajorHeadExists(string majorAccCode, CancellationToken ct)
        {
            return await _context.MajorHeads.AnyAsync(m => m.MajorAccCode == majorAccCode, ct);
        }

        private async Task<bool> SubMajorHeadExists(string subMajorAccCode, CancellationToken ct)
        {
            return await _context.SubMajorHeads.AnyAsync(s => s.SubMajorAccCode == subMajorAccCode, ct);
        }

        private async Task<bool> MinorHeadExists(string minorAccCode, CancellationToken ct)
        {
            return await _context.MinorHeads.AnyAsync(m => m.MinorAccCode == minorAccCode, ct);
        }
    }
}
