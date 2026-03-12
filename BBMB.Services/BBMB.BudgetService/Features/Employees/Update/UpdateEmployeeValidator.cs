using FluentValidation;

namespace BBMB.BudgetService.Features.Employees.Update
{
    public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
    {
        public UpdateEmployeeValidator()
        {
            RuleFor(x => x.dto.PersonalInfo.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.dto.PersonalInfo.NameEnglish)
                .NotEmpty()
                .WithMessage("Employee name is required");

            RuleFor(x => x.dto.PersonalInfo.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.dto.PersonalInfo.PhoneNumber)
                .NotEmpty();

            RuleFor(x => x.dto.Employment.DesignationId)
                .NotEmpty();

            RuleFor(x => x.dto.Employment.AppointmentDate)
                .NotEmpty();
        }
    }
}
