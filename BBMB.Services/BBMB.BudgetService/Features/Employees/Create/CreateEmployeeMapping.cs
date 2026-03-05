using BBMB.BudgetService.Features.Employees.Create.Dtos;
using BBMB.BudgetService.Features.Employees.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Employees.Create
{
    public class CreateEmployeeMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateEmployeeDto, Employee>()
                .Map(dest => dest.UserId, src => src.PersonalInfo.UserId)
                .Map(dest => dest.ApplicantTitle, src => src.PersonalInfo.ApplicantTitle)
                .Map(dest => dest.NameEnglish, src => src.PersonalInfo.NameEnglish)
                .Map(dest => dest.NameHindi, src => src.PersonalInfo.NameHindi)
                .Map(dest => dest.Gender, src => src.PersonalInfo.Gender)
                .Map(dest => dest.DateOfBirth, src => src.PersonalInfo.DateOfBirth)
                .Map(dest => dest.PhoneNumber, src => src.PersonalInfo.PhoneNumber)
                .Map(dest => dest.Email, src => src.PersonalInfo.Email)
                .Map(dest => dest.Address, src => src.PersonalInfo.Address);
        }
    }
}
