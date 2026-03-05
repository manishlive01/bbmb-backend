using FluentValidation;

namespace BBMB.BudgetService.Features.Employees.GetList
{
    public class GetEmployeeListValidator : AbstractValidator<GetEmployeeListQuery>
    {
        public GetEmployeeListValidator()
        {
            RuleFor(x => x.dto.PageNumber)
                .GreaterThan(0);

            RuleFor(x => x.dto.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);
        }
    }
}
