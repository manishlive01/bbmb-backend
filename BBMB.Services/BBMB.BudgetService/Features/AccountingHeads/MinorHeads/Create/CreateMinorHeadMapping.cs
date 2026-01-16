using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create.Dtos;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create
{
    public static class CreateMinorHeadMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<CreateMinorHeadDto, MinorHead>
                .NewConfig()

                // ===== Simple direct mappings =====
                .Map(dest => dest.MinorAccCode,
                     src => src.MinorAccCode.Trim().ToUpper())

                .Map(dest => dest.MinorAccDesc,
                     src => src.MinorAccDesc.Trim())

                // ===== Ignore DB / system-controlled fields =====
                .Ignore(dest => dest.UserCode)
                .Ignore(dest => dest.ModificationDate);
        }
    }
}
