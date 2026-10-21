using BBMB.BudgetService.Features.Works.Update.Dtos;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;

namespace BBMB.BudgetService.Features.Works.Update
{
    public class UpdateWorkValidator : AbstractValidator<UpdateWorkCommand>
    {
        public UpdateWorkValidator()
        {
            // ================= REQUIRED FIELDS =================

            RuleFor(x => x.dto.WorkDesc)
                .NotEmpty().WithMessage("Work description is required.")
                .MaximumLength(500).WithMessage("Work description must not exceed 500 characters.");

            //RuleFor(x => x.dto.DivisionId)
            //    .NotEmpty().WithMessage("Division code is required.");

            //RuleFor(x => x.dto.DivSection)
            //    .NotEmpty().WithMessage("Division section is required.");

            // Design §5.2, §11.3: the head and class are optional; the handler applies the reference rule.
            RuleFor(x => x.dto.AccountCodeId)
                .GreaterThan(0).When(x => x.dto.AccountCodeId.HasValue)
                .WithMessage("Account code id must be positive.");
            RuleFor(x => x.dto.WorkClass)
                .Must(c => c is "AA" or "AMR").When(x => x.dto.WorkClass is not null)
                .WithMessage("Work class must be AA or AMR.");
            RuleFor(x => x.dto.PrecedingAmrWorkId)
                .GreaterThan(0).When(x => x.dto.PrecedingAmrWorkId.HasValue)
                .WithMessage("Preceding AM&R work id must be positive.");
            RuleFor(x => x.dto.CECode)
                .NotEmpty().WithMessage("CE code is required.");

            RuleFor(x => x.dto.CRCode)
                .NotEmpty().WithMessage("CR code is required.");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required.")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters.");

            // ================= OPTIONAL FIELDS =================

            RuleFor(x => x.dto.FinYrStart)
                .MaximumLength(4).WithMessage("Financial year start must not exceed 4 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.FinYrStart));

            RuleFor(x => x.dto.FinYrEnd)
                .MaximumLength(4).WithMessage("Financial year end must not exceed 4 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.FinYrEnd));

            RuleFor(x => x.dto.SanctionNo)
                .MaximumLength(20).WithMessage("Sanction number must not exceed 20 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.SanctionNo));

            RuleFor(x => x.dto.SanctionDate)
                .MaximumLength(11).WithMessage("Sanction date must not exceed 11 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.SanctionDate));

            RuleFor(x => x.dto.DesignationCode)
                .MaximumLength(6).WithMessage("Designation code must not exceed 6 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.DesignationCode));

            RuleFor(x => x.dto.Status)
                .MaximumLength(1).WithMessage("Status must not exceed 1 character.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.Status));

            RuleFor(x => x.dto.Nature)
                .MaximumLength(1).WithMessage("Nature must not exceed 1 character.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.Nature));

            RuleFor(x => x.dto.RefWorkCode)
                .MaximumLength(8).WithMessage("Reference work code must not exceed 8 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.RefWorkCode));

            RuleFor(x => x.dto.OriginalBudget)
                .MaximumLength(50).WithMessage("Original budget must not exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.OriginalBudget));

            RuleFor(x => x.dto.RevisedBudget)
                .MaximumLength(50).WithMessage("Revised budget must not exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.RevisedBudget));

            RuleFor(x => x.dto.ProposedBudget)
                .MaximumLength(50).WithMessage("Proposed budget must not exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.ProposedBudget));

            RuleFor(x => x.dto.Remarks)
                .MaximumLength(500).WithMessage("Remarks must not exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.dto.Remarks));

            RuleFor(x => x.dto.EstimatedCost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Estimated cost cannot be negative.")
                .When(x => x.dto.EstimatedCost.HasValue);
            RuleFor(x => x.dto.EstimatedCost).WholeRupees(); // BUD-013, design §4.5 step 4
        }
    }
}