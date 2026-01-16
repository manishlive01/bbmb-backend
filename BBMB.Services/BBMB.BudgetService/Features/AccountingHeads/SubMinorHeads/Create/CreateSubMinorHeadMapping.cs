using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Create.Dtos;
using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Create
{
    public static class CreateSubMinorHeadMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<CreateSubMinorHeadDto, SubMinorHead>
                .NewConfig()

                // ===== Simple direct mappings =====
                .Map(dest => dest.MinorAccCode,
                     src => src.MinorAccCode)

                .Map(dest => dest.SubMinorAccCode,
                     src => src.SubMinorAccCode)

                .Map(dest => dest.SubMinorAccDesc,
                     src => src.SubMinorAccDesc)


                // ===== Ignore DB / system fields =====
                .Ignore(dest => dest.UserCode)
                .Ignore(dest => dest.ModificationDate);
        }
    }
}
