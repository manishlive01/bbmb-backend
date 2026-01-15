using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update.Dtos;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update
{
    public static class UpdateSubMajorHeadMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<UpdateSubMajorHeadDto, SubMajorHead>
                .NewConfig()

                // ===== Direct mappings =====
                .Map(dest => dest.MajorAccCode,
                     src => src.MajorAccCode)

                .Map(dest => dest.SubMajorAccCode,
                     src => src.SubMajorAccCode)

                .Map(dest => dest.SubMajorAccDesc,
                     src => src.SubMajorAccDesc)

                // ===== Ignore system-controlled fields =====
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.ModificationDate)
                .Ignore(dest => dest.UserCode);
        }
    }
}
