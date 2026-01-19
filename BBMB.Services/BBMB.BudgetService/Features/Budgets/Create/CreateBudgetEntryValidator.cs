using BBMB.BudgetService.Features.Budgets.Create.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.Budgets.Create
{
    public class CreateBudgetEntryValidator : AbstractValidator<CreateBudgetEntryCommand>
    {
        public CreateBudgetEntryValidator()
        {
            RuleFor(x => x.dto.DivisionCode)
                .NotEmpty().WithMessage("Division code is required");

            RuleFor(x => x.dto.DivSection)
                .NotEmpty().WithMessage("Division section is required");

            RuleFor(x => x.dto.CalendarYear)
                .NotEmpty().WithMessage("Calendar year is required")
                .GreaterThan(1900).WithMessage("Calendar year must be greater than 1900")
                .LessThanOrEqualTo(2100).WithMessage("Calendar year must be less than or equal to 2100");

            RuleFor(x => x.dto.SequenceNo)
                .NotEmpty().WithMessage("Sequence number is required");

            RuleFor(x => x.dto.MajorAccCode)
                .NotEmpty().WithMessage("Major account code is required")
                .MaximumLength(4).WithMessage("Major account code must not exceed 4 characters");

            RuleFor(x => x.dto.SubMajorAccCode)
                .NotEmpty().WithMessage("Sub-major account code is required")
                .MaximumLength(2).WithMessage("Sub-major account code must not exceed 2 characters");

            RuleFor(x => x.dto.MinorAccCode)
                .NotEmpty().WithMessage("Minor account code is required")
                .MaximumLength(3).WithMessage("Minor account code must not exceed 3 characters");

            RuleFor(x => x.dto.SubMinorAccCode)
                .NotEmpty().WithMessage("Sub-minor account code is required")
                .MaximumLength(2).WithMessage("Sub-minor account code must not exceed 2 characters");

            RuleFor(x => x.dto.DetailAccCode)
                .NotEmpty().WithMessage("Detail account code is required")
                .MaximumLength(3).WithMessage("Detail account code must not exceed 3 characters");

            RuleFor(x => x.dto.ObjectAccCode)
                .NotEmpty().WithMessage("Object account code is required")
                .MaximumLength(3).WithMessage("Object account code must not exceed 3 characters");

            RuleFor(x => x.dto.WorkCode)
                .MaximumLength(8).WithMessage("Work code must not exceed 8 characters")
                .When(x => !string.IsNullOrEmpty(x.dto.WorkCode));

            RuleFor(x => x.dto.BudgetDescription)
                .NotEmpty().WithMessage("Budget description is required")
                .MaximumLength(100).WithMessage("Budget description must not exceed 100 characters");

            RuleFor(x => x.dto.BudgetType)
                .IsInEnum().WithMessage("Budget type must be Work, Expensive, or Receipt");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
