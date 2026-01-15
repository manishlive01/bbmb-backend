using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create.Dtos;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create
{
    public static class CreateSubMajorHeadMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<CreateSubMajorHeadDto, SubMajorHead>
                .NewConfig()

                // ===== Simple direct mappings =====
                .Map(dest => dest.MajorAccCode,
                     src => src.MajorAccCode.Trim().ToUpper())

                .Map(dest => dest.SubMajorAccCode,
                     src => src.SubMajorAccCode.Trim().ToUpper())

                .Map(dest => dest.SubMajorAccDesc,
                     src => src.SubMajorAccDesc.Trim())


                // ===== Ignore DB / system-controlled fields =====
                .Ignore(dest => dest.ModificationDate);
        }
    }
}
