using BBMB.BudgetService.Features.Designations.Entities;
using BBMB.BudgetService.Features.Designations.Update.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Designations.Update
{
    public sealed class UpdateDesignationMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateDesignationDto, Designation>()
                // 🔑 Key is NOT mapped (used only for lookup)
                .Ignore(dest => dest.DesignationCode)

                // ✅ Updatable fields
                .Map(dest => dest.DesignationDesc, src => src.DesignationDesc)
                .Map(dest => dest.ClassCode, src => src.ClassCode)
                .Map(dest => dest.MinBasic, src => src.MinBasic)
                .Map(dest => dest.MaxBasic, src => src.MaxBasic)
                .Map(dest => dest.UserCode, src => src.UserCode)

                // 🕒 System fields
                .Map(dest => dest.ModificationDate, _ => DateTime.UtcNow);
        }
    }
}
