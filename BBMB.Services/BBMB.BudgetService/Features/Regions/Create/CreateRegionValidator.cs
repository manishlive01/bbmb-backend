using BBMB.BudgetService.Features.Regions.Create.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.Regions.Create
{
    public class CreateRegionValidator : AbstractValidator<CreateRegionDto>
    {
        public CreateRegionValidator()
        {
            RuleFor(x => x.RegionCode)
                .NotEmpty().WithMessage("Region code is required")
                .Length(1).WithMessage("Region code must be exactly 1 character");

            RuleFor(x => x.RegionDesc)
                .NotEmpty().WithMessage("Region description is required")
                .MaximumLength(50).WithMessage("Region description must not exceed 50 characters");

            RuleFor(x => x.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
