using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create
{
    public class CreateMinorHeadValidator : AbstractValidator<CreateMinorHeadCommand>
    {
        private readonly BudgetContext _context;

        public CreateMinorHeadValidator(BudgetContext context)
        {
            _context = context;

            RuleFor(x => x.dto.MinorAccCode)
                .NotEmpty().WithMessage("Minor account code is required")
                .MaximumLength(3).WithMessage("Minor account code must not exceed 3 characters");

            //RuleFor(x => x.MajorAccCode)
            //    .NotEmpty().WithMessage("Major account code is required")
            //    .MaximumLength(4).WithMessage("Major account code must not exceed 4 characters")
            //    .MustAsync(async (code, ct) => await _context.MajorHeads.AnyAsync(m => m.MajorAccCode == code, ct))
            //    .WithMessage("Major account code does not exist");

            //RuleFor(x => x.SubMajorAccCode)
            //    .NotEmpty().WithMessage("Sub-major account code is required")
            //    .MaximumLength(2).WithMessage("Sub-major account code must not exceed 2 characters")
            //    .MustAsync(async (code, ct) => await _context.SubMajorHeads.AnyAsync(s => s.SubMajorAccCode == code, ct))
            //    .WithMessage("Sub-major account code does not exist");

            RuleFor(x => x.dto.MinorAccDesc)
                .NotEmpty().WithMessage("Minor account description is required")
                .MaximumLength(100).WithMessage("Minor account description must not exceed 100 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
