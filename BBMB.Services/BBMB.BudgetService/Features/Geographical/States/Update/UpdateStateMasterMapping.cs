using Mapster;
using BBMB.BudgetService.Features.Geographical.States.Update.Dtos;
using BBMB.BudgetService.Features.Geographical.States.Entities;

namespace BBMB.BudgetService.Features.Geographical.States.Update
{
    public static class UpdateStateMasterMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<UpdateStateMasterDto, StateMaster>
                .NewConfig()

                // ===== Allowed updates =====
                .Map(dest => dest.StateCode,
                     src => src.StateCode.Trim().ToUpper())

                .Map(dest => dest.StateName,
                     src => src.StateName.Trim())

                .Map(dest => dest.IsActive,
                     src => src.IsActive)

                // ===== Never update these =====
                .Ignore(dest => dest.StateMasterId)
                .Ignore(dest => dest.IsDeleted)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.DeletedAt);
        }
    }
}
