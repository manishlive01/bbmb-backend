using BBMB.BudgetService.Features.Works.Update.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.Works.Update
{
    public class UpdateWorkValidator : AbstractValidator<UpdateWorkDto>
    {
        public UpdateWorkValidator()
        {
            // ================= REQUIRED FIELDS =================

            RuleFor(x => x.WorkDesc)
                .NotEmpty().WithMessage("Work description is required.")
                .MaximumLength(200).WithMessage("Work description must not exceed 200 characters.");

            RuleFor(x => x.DivisionCode)
                .NotEmpty().WithMessage("Division code is required.");

            RuleFor(x => x.DivSection)
                .NotEmpty().WithMessage("Division section is required.");

            RuleFor(x => x.MajorAccCode)
                .NotEmpty().WithMessage("Major account code is required.")
                .MaximumLength(4).WithMessage("Major account code must not exceed 4 characters.");

            RuleFor(x => x.SubMajorAccCode)
                .NotEmpty().WithMessage("Sub-major account code is required.")
                .MaximumLength(2).WithMessage("Sub-major account code must not exceed 2 characters.");

            RuleFor(x => x.MinorAccCode)
                .NotEmpty().WithMessage("Minor account code is required.")
                .MaximumLength(3).WithMessage("Minor account code must not exceed 3 characters.");

            RuleFor(x => x.SubMinorAccCode)
                .NotEmpty().WithMessage("Sub-minor account code is required.")
                .MaximumLength(2).WithMessage("Sub-minor account code must not exceed 2 characters.");

            RuleFor(x => x.DetailAccCode)
                .NotEmpty().WithMessage("Detail account code is required.")
                .MaximumLength(3).WithMessage("Detail account code must not exceed 3 characters.");

            RuleFor(x => x.ObjectAccCode)
                .NotEmpty().WithMessage("Object account code is required.")
                .MaximumLength(3).WithMessage("Object account code must not exceed 3 characters.");

            RuleFor(x => x.CECode)
                .NotEmpty().WithMessage("CE code is required.");

            RuleFor(x => x.CRCode)
                .NotEmpty().WithMessage("CR code is required.");

            RuleFor(x => x.UserCode)
                .NotEmpty().WithMessage("User code is required.")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters.");

            // ================= OPTIONAL FIELDS =================

            RuleFor(x => x.FinYrStart)
                .MaximumLength(4).WithMessage("Financial year start must not exceed 4 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.FinYrStart));

            RuleFor(x => x.FinYrEnd)
                .MaximumLength(4).WithMessage("Financial year end must not exceed 4 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.FinYrEnd));

            RuleFor(x => x.SanctionNo)
                .MaximumLength(20).WithMessage("Sanction number must not exceed 20 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.SanctionNo));

            RuleFor(x => x.SanctionDate)
                .MaximumLength(11).WithMessage("Sanction date must not exceed 11 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.SanctionDate));

            RuleFor(x => x.DesignationCode)
                .MaximumLength(6).WithMessage("Designation code must not exceed 6 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.DesignationCode));

            RuleFor(x => x.Status)
                .MaximumLength(1).WithMessage("Status must not exceed 1 character.")
                .When(x => !string.IsNullOrWhiteSpace(x.Status));

            RuleFor(x => x.Nature)
                .MaximumLength(1).WithMessage("Nature must not exceed 1 character.")
                .When(x => !string.IsNullOrWhiteSpace(x.Nature));

            RuleFor(x => x.RefWorkCode)
                .MaximumLength(8).WithMessage("Reference work code must not exceed 8 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.RefWorkCode));

            RuleFor(x => x.OriginalBudget)
                .MaximumLength(50).WithMessage("Original budget must not exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.OriginalBudget));

            RuleFor(x => x.RevisedBudget)
                .MaximumLength(50).WithMessage("Revised budget must not exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.RevisedBudget));

            RuleFor(x => x.ProposedBudget)
                .MaximumLength(50).WithMessage("Proposed budget must not exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.ProposedBudget));

            RuleFor(x => x.Remarks)
                .MaximumLength(500).WithMessage("Remarks must not exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Remarks));

            RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Estimated cost cannot be negative.")
                .When(x => x.EstimatedCost.HasValue);
        }
    }
}