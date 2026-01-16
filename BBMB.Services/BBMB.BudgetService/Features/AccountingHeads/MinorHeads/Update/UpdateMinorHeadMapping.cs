using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Update.Dtos;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Update
{
    public static class UpdateMinorHeadMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<UpdateMinorHeadDto, MinorHead>
                .NewConfig()

                // ===== Allow updating code & description =====
                .Map(dest => dest.MinorAccCode,
                     src => src.MinorAccCode)

                .Map(dest => dest.MinorAccDesc,
                     src => src.MinorAccDesc)


                // ===== Ignore system-controlled fields =====
                .Ignore(dest => dest.UserCode)
                .Ignore(dest => dest.ModificationDate);
        }
    }
}
