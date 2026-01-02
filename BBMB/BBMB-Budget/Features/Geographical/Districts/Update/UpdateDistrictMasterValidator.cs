using FluentValidation;

namespace BBMB_Budget.Features.Geographical.Districts.Update
{
    public class UpdateDistrictMasterValidator
        : AbstractValidator<UpdateDistrictMasterCommand>
    {
        public UpdateDistrictMasterValidator()
        {
            // ==========================
            // StateMasterId Validation
            // ==========================
            RuleFor(x => x.dto.StateMasterId)
                .NotEmpty()
                .WithMessage("State is required.");

            // ==========================
            // District Code Validation
            // ==========================
            RuleFor(x => x.dto.DistrictCode)
                .NotEmpty()
                .WithMessage("District code is required.")
                .Length(2, 5)
                .WithMessage("District code must be between 2 and 5 characters.")
                .WithMessage("District code must contain only uppercase letters.");

            // ==========================
            // District Name Validation
            // ==========================
            RuleFor(x => x.dto.DistrictName)
                .NotEmpty()
                .WithMessage("District name is required.")
                .MaximumLength(100)
                .WithMessage("District name cannot exceed 100 characters.")
                .Matches("^[A-Za-z ]+$")
                .WithMessage("District name must contain only alphabet.");

            // ==========================
            // IsActive Validation
            // ==========================
            RuleFor(x => x.dto.IsActive)
                .NotNull()
                .WithMessage("IsActive flag must be specified.");
        }
    }
}
