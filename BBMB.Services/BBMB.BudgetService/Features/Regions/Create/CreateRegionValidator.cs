using FluentValidation;

namespace BBMB.BudgetService.Features.Regions.Create
{
    public class CreateRegionValidator : AbstractValidator<CreateRegionCommand>
    {
        public CreateRegionValidator()
        {
            RuleFor(x => x.dto.RegionCode)
                .NotEmpty().WithMessage("Region code is required")
                .MaximumLength(2).WithMessage("Region code must be maximum 2 characters");

            RuleFor(x => x.dto.RegionDesc)
                .NotEmpty().WithMessage("Region description is required")
                .MaximumLength(50).WithMessage("Region description must not exceed 50 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
