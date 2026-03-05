using BBMB.BudgetService.Features.Employees.Entities;
using BBMB.BudgetService.Features.Employees.GetById.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.Employees.GetById
{
    public class GetEmployeeByUserIdMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Employee, GetEmployeeResponseDto>()
                .Map(dest => dest.PersonalInfo, src => src)
                .Map(dest => dest.Employment, src => src.EmploymentDetail)
                .Map(dest => dest.PhotoDocs.PhotoUrl, src => src.PhotoDocument.PhotoUrl)
                .Map(dest => dest.FamilyMembers, src => src.FamilyMembers);

            config.NewConfig<Employee, PersonalInfoResponseDto>();

            config.NewConfig<EmploymentDetail, EmploymentInfoResponseDto>();

            config.NewConfig<FamilyMember, FamilyMemberResponseDto>();
        }
    }
}
