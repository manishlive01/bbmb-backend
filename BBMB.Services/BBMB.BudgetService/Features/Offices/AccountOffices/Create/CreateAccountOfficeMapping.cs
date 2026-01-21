using BBMB.BudgetService.Features.Offices.AccountOffices.Create.Dtos;
using BBMB.BudgetService.Features.Offices.AccountOffices.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Offices.AccountOffices.Create
{
    public static class CreateAccountOfficeMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<CreateAccountOfficeDto, AccountOffice>
                .NewConfig()

                // ===== Direct mappings =====
                .Map(dest => dest.AccOffice,
                     src => src.AccOffice.Trim().ToUpper())

                .Map(dest => dest.AccOfficeDesc,
                     src => src.AccOfficeDesc.Trim())

                // ===== Ignore system-controlled fields =====
                .Ignore(dest => dest.ModificationDate)
                .Ignore(dest => dest.UserCode);
        }
    }
}
