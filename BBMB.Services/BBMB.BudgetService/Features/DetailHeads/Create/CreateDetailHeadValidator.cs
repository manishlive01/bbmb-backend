using BBMB.BudgetService.Features.DetailHeads.Create.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.DetailHeads.Create
{
    public class CreateDetailHeadValidator : AbstractValidator<CreateDetailHeadDto>
    {
        private readonly BudgetContext _context;

        public CreateDetailHeadValidator(BudgetContext context)
        {
            _context = context;

            RuleFor(x => x.DetailAccCode)
                .NotEmpty().WithMessage("Detail Account Code is required.")
                .MaximumLength(2).WithMessage("Detail Account Code must not exceed 2 characters.");

            RuleFor(x => x.MajorAccCode)
                .NotEmpty().WithMessage("Major Account Code is required.")
                .MaximumLength(4).WithMessage("Major Account Code must not exceed 4 characters.")
                .MustAsync(MajorHeadExists).WithMessage("Major Head with code '{PropertyValue}' does not exist.");

            RuleFor(x => x.SubMajorAccCode)
                .NotEmpty().WithMessage("Sub-Major Account Code is required.")
                .MaximumLength(2).WithMessage("Sub-Major Account Code must not exceed 2 characters.")
                .MustAsync(SubMajorHeadExists).WithMessage("Sub-Major Head with code '{PropertyValue}' does not exist.");

            RuleFor(x => x.MinorAccCode)
                .NotEmpty().WithMessage("Minor Account Code is required.")
                .MaximumLength(3).WithMessage("Minor Account Code must not exceed 3 characters.")
                .MustAsync(MinorHeadExists).WithMessage("Minor Head with code '{PropertyValue}' does not exist.");

            RuleFor(x => x.SubMinorAccCode)
                .NotEmpty().WithMessage("Sub-Minor Account Code is required.")
                .MaximumLength(2).WithMessage("Sub-Minor Account Code must not exceed 2 characters.")
                .MustAsync(SubMinorHeadExists).WithMessage("Sub-Minor Head with code '{PropertyValue}' does not exist.");

            RuleFor(x => x.DetailAccDesc)
                .NotEmpty().WithMessage("Detail Account Description is required.")
                .MaximumLength(100).WithMessage("Detail Account Description must not exceed 100 characters.");

            RuleFor(x => x.UserCode)
                .NotEmpty().WithMessage("User Code is required.")
                .MaximumLength(6).WithMessage("User Code must not exceed 6 characters.");
        }

        private async Task<bool> MajorHeadExists(string majorAccCode, CancellationToken cancellationToken)
        {
            return await _context.MajorHeads.AnyAsync(m => m.MajorAccCode == majorAccCode, cancellationToken);
        }

        private async Task<bool> SubMajorHeadExists(string subMajorAccCode, CancellationToken cancellationToken)
        {
            return await _context.SubMajorHeads.AnyAsync(s => s.SubMajorAccCode == subMajorAccCode, cancellationToken);
        }

        private async Task<bool> MinorHeadExists(string minorAccCode, CancellationToken cancellationToken)
        {
            return await _context.MinorHeads.AnyAsync(m => m.MinorAccCode == minorAccCode, cancellationToken);
        }

        private async Task<bool> SubMinorHeadExists(string subMinorAccCode, CancellationToken cancellationToken)
        {
            return await _context.SubMinorHeads.AnyAsync(s => s.SubMinorAccCode == subMinorAccCode, cancellationToken);
        }
    }
}
