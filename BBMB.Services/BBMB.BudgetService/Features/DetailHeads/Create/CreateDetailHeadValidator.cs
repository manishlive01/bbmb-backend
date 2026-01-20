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

           
            RuleFor(x => x.DetailAccDesc)
                .NotEmpty().WithMessage("Detail Account Description is required.")
                .MaximumLength(100).WithMessage("Detail Account Description must not exceed 100 characters.");

        }
    }
}
