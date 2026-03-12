using BBMB.BudgetService.Features.Employees.Entities;
using BBMB.BudgetService.Features.Employees.GetList.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.Employees.GetList
{
    public class GetEmployeeListMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Employee, GetEmployeeListResponseDto>()
                .Map(dest => dest.EmployeeId, src => src.Id)
                .Map(dest => dest.DesignationId, src => src.EmploymentDetail.DesignationId)
                .Map(dest => dest.DateOfJoining, src => src.EmploymentDetail.DateOfJoining)
                .Map(dest => dest.PhotoUrl, src => src.PhotoDocument.PhotoUrl);
        }
    }
}
