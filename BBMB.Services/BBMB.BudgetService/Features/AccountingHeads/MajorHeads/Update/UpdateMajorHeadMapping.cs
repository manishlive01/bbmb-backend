using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Update.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Update
{
    public class UpdateMajorHeadMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateMajorHeadDto, MajorHead>()
                // 🔑 Key is used only for lookup, not overwritten
                .Ignore(dest => dest.MajorAccCode)

                // ✅ Updatable fields
                .Map(dest => dest.MajorAccDesc, src => src.MajorAccDesc)
                .Map(dest => dest.UserCode, src => src.UserCode)

                // 🔒 System-managed fields
                .Ignore(dest => dest.ModificationDate)

                // 🔒 Navigation properties
                .Ignore(dest => dest.SubMajorHeads);
        }
    }
}
