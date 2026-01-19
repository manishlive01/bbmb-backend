using FluentValidation;

namespace BBMB.BudgetService.Features.Pools.Create
{
    public class CreatePoolValidator : AbstractValidator<CreatePoolCommand>
    {
        public CreatePoolValidator()
        {
            RuleFor(x => x.dto.PoolCode)
                .NotEmpty().WithMessage("Pool code is required")
                .MaximumLength(2).WithMessage("Pool code must be maximum 2 characters");

            RuleFor(x => x.dto.PoolDesc)
                .NotEmpty().WithMessage("Pool description is required")
                .MaximumLength(50).WithMessage("Pool description must not exceed 50 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
