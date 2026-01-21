using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update
{
    public class UpdateSubMajorHeadValidator : AbstractValidator<UpdateSubMajorHeadCommand>
    {
        private readonly BudgetContext _context;
        
        public UpdateSubMajorHeadValidator(BudgetContext context)
        {
            _context = context;
            
            RuleFor(x => x.dto.MajorAccCode)
                .NotEmpty().WithMessage("Major account code is required")
                .MaximumLength(4).WithMessage("Major account code must not exceed 4 characters")
                .MustAsync(async (code, ct) => await _context.MajorHeads.AnyAsync(m => m.MajorAccCode == code, ct))
                .WithMessage("Major account code does not exist");
                
            RuleFor(x => x.dto.SubMajorAccDesc)
                .NotEmpty().WithMessage("Sub-major account description is required")
                .MaximumLength(100).WithMessage("Sub-major account description must not exceed 100 characters");
                
            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
