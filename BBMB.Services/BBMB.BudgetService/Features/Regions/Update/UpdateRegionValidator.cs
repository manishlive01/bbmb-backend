using FluentValidation;

namespace BBMB.BudgetService.Features.Regions.Update
{
    public class UpdateRegionValidator : AbstractValidator<UpdateRegionCommand>
    {
        public UpdateRegionValidator()
        {
            RuleFor(x => x.dto.RegionDesc)
                .NotEmpty().WithMessage("Region description is required")
                .MaximumLength(50).WithMessage("Region description must not exceed 50 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
