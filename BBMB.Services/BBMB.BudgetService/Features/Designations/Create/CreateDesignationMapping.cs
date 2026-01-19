using BBMB.BudgetService.Features.Designations.Create.Dtos;
using BBMB.BudgetService.Features.Designations.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Designations.Create
{
    public class CreateDesignationMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateDesignationDto, Designation>()

                // ===== Normalize codes =====
                .Map(dest => dest.DesignationCode,
                     src => src.DesignationCode.Trim().ToUpper())

                .Map(dest => dest.DesignationDesc,
                     src => src.DesignationDesc.Trim())

                .Map(dest => dest.ClassCode,
                     src => src.ClassCode.Trim().ToUpper())

                // ===== Salary fields =====
                .Map(dest => dest.MinBasic,
                     src => src.MinBasic)

                .Map(dest => dest.MaxBasic,
                     src => src.MaxBasic)

                // ===== System fields =====
                .Map(dest => dest.ModificationDate,
                     src => DateTime.UtcNow)

                // UserCode should be set in handler (logged-in user)
                .Ignore(dest => dest.UserCode)

                // Navigation property handled by EF
                .Ignore(dest => dest.Class);
        }
    }
}
