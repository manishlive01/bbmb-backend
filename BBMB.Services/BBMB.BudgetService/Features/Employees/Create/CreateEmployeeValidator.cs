using FluentValidation;

namespace BBMB.BudgetService.Features.Employees.Create
{
    public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
    {
        public CreateEmployeeValidator()
        {
            // Root DTO
            RuleFor(x => x.dto)
                .NotNull()
                .WithMessage("Request body is required");

            //------------------------------------
            // Personal Info Validation
            //------------------------------------

            RuleFor(x => x.dto.PersonalInfo.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.dto.PersonalInfo.TitleId)
               .NotEmpty()
               .WithMessage("Title is required");

            RuleFor(x => x.dto.PersonalInfo.NameEnglish)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Employee name is required");

            RuleFor(x => x.dto.PersonalInfo.NameHindi)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.dto.PersonalInfo.NameHindi));

            RuleFor(x => x.dto.PersonalInfo.GenderId)
                .NotEmpty()
                .WithMessage("Gender is required");

            RuleFor(x => x.dto.PersonalInfo.DateOfBirth)
                .LessThan(DateTime.Today)
                .WithMessage("Date of birth must be in the past");

            RuleFor(x => x.dto.PersonalInfo.PhoneNumber)
                .NotEmpty()
                .Matches(@"^[0-9]{10}$")
                .WithMessage("Phone number must be 10 digits");

            RuleFor(x => x.dto.PersonalInfo.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Valid email is required");

            RuleFor(x => x.dto.PersonalInfo.Address)
                .NotEmpty()
                .MaximumLength(250);

            //------------------------------------
            // Employment Validation
            //------------------------------------


            RuleFor(x => x.dto.Employment.SourceOfRecruitmentId)
     .NotEmpty()
     .WithMessage("Source Of Recruitment is required");

            RuleFor(x => x.dto.Employment.DesignationId)
                .NotEmpty()
                .WithMessage("Designation is required");

            RuleFor(x => x.dto.Employment.Position)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Position is required and cannot exceed 100 characters");

            RuleFor(x => x.dto.Employment.AppointmentTypeId)
                .NotEmpty()
                .WithMessage("Appointment Type is required");

            RuleFor(x => x.dto.Employment.AppointmentDate)
                .NotEmpty();

            RuleFor(x => x.dto.Employment.DateOfJoining)
                .NotEmpty()
                .GreaterThanOrEqualTo(x => x.dto.Employment.AppointmentDate)
                .WithMessage("Joining date must be after appointment date");

            RuleFor(x => x.dto.Employment.RetirementDate)
                .GreaterThan(x => x.dto.Employment.DateOfJoining)
                .When(x => x.dto.Employment.RetirementDate.HasValue)
                .WithMessage("Retirement date must be after joining date");

            //------------------------------------
            // Photo Validation
            //------------------------------------

            RuleFor(x => x.dto.PhotoDocs.Photo)
                .NotNull()
                .WithMessage("Employee photo is required");

            //------------------------------------
            // Family Members Validation
            //------------------------------------

            RuleForEach(x => x.dto.FamilyMembers).ChildRules(family =>
            {
                family.RuleFor(x => x.Name)
                    .NotEmpty()
                    .MaximumLength(100);

                family.RuleFor(x => x.Relation)
                    .NotEmpty()
                    .MaximumLength(50);

                family.RuleFor(x => x.PhoneNumber)
                    .NotEmpty()
                    .Matches(@"^[0-9]{10}$")
                    .WithMessage("Phone number must be 10 digits");

                family.RuleFor(x => x.Address)
                    .NotEmpty()
                    .MaximumLength(250);

                family.RuleFor(x => x.Photo)
                    .Must(file => file == null || file.Length > 0)
                    .WithMessage("Invalid photo file");
            });
        }
    }
}