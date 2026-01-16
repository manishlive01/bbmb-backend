using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Update.Dtos;
using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Update
{
    public static class UpdateSubMinorHeadMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<UpdateSubMinorHeadDto, SubMinorHead>
                .NewConfig()

                // ===== Allow updating codes =====
                .Map(dest => dest.MinorAccCode,
                     src => src.MinorAccCode.Trim().ToUpper())

                .Map(dest => dest.SubMinorAccCode,
                     src => src.SubMinorAccCode.Trim().ToUpper())

                .Map(dest => dest.SubMinorAccDesc,
                     src => src.SubMinorAccDesc.Trim())

                // ===== Ignore system-controlled fields =====
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.ModificationDate)
                .Ignore(dest => dest.UserCode);
        }
    }
}
