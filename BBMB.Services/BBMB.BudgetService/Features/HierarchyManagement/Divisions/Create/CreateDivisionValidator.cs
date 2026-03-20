using FluentValidation;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.Create
{
    public class CreateDivisionValidator : AbstractValidator<CreateDivisionCommand>
    {
        public CreateDivisionValidator()
        {
            RuleFor(x => x.dto.DivisionCode)
                .NotEmpty().WithMessage("Division code is required");
             

            RuleFor(x => x.dto.DivisionDesc)
                .NotEmpty().WithMessage("Division description is required")
                .MaximumLength(50).WithMessage("Division description must not exceed 50 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");

            RuleFor(x => x.dto.RoCode)
                .MaximumLength(3).WithMessage("RO code must not exceed 3 characters")
                .When(x => !string.IsNullOrEmpty(x.dto.RoCode));

            RuleFor(x => x.dto.MajorAccCode)
                .MaximumLength(4).WithMessage("Major account code must not exceed 4 characters")
                .When(x => !string.IsNullOrEmpty(x.dto.MajorAccCode));

            RuleFor(x => x.dto.AccOffice)
                .MaximumLength(2).WithMessage("Account office must not exceed 2 characters")
                .When(x => !string.IsNullOrEmpty(x.dto.AccOffice));

            RuleFor(x => x.dto.UnitCode)
                .MaximumLength(2).WithMessage("Unit code must not exceed 2 characters")
                .When(x => !string.IsNullOrEmpty(x.dto.UnitCode));

            RuleFor(x => x.dto.PoolCode)
                .MaximumLength(1).WithMessage("Pool code must not exceed 1 character")
                .When(x => !string.IsNullOrEmpty(x.dto.PoolCode));

            RuleFor(x => x.dto.UmdUnit)
                .MaximumLength(3).WithMessage("UMD unit must not exceed 3 characters")
                .When(x => !string.IsNullOrEmpty(x.dto.UmdUnit));

            RuleFor(x => x.dto.RegionCode)
                .MaximumLength(1).WithMessage("Region code must not exceed 1 character")
                .When(x => !string.IsNullOrEmpty(x.dto.RegionCode));

            RuleFor(x => x.dto.DdoTds)
                .MaximumLength(15).WithMessage("DDO TDS must not exceed 15 characters")
                .When(x => !string.IsNullOrEmpty(x.dto.DdoTds));
        }
    }
}
