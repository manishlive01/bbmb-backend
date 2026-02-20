using FluentValidation;

namespace BBMB.BudgetService.Features.CRManagements.Update
{
    public class UpdateCRManagementValidator
        : AbstractValidator<UpdateCRManagementCommand>
    {
        public UpdateCRManagementValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Valid CR Id is required");

            RuleFor(x => x.Dto.CEId)
                .GreaterThan(0)
                .WithMessage("Valid CE Id is required");

            RuleFor(x => x.Dto.Name)
                .NotEmpty()
                .MaximumLength(15);

            RuleFor(x => x.Dto.Description)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.Dto.Description));

            RuleFor(x => x.Dto.Location)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.Dto.Location));
        }
    }
}
