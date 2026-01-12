using BBMB.BudgetService.Features.Geographical.States.Create.Dtos;
using BBMB.BudgetService.Features.Geographical.States.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Geographical.States.Create
{
    public static class CreateStateMasterMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<CreateStateMasterDto, StateMaster>
                .NewConfig()

                // ===== Simple direct mappings =====
                .Map(dest => dest.StateCode,
                     src => src.StateCode.Trim().ToUpper())

                .Map(dest => dest.StateName,
                     src => src.StateName.Trim())

                .Map(dest => dest.IsActive,
                     src => src.IsActive)

                // ===== Ignore DB / system-controlled fields =====
                .Ignore(dest => dest.StateMasterId)
                .Ignore(dest => dest.IsDeleted)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.DeletedAt);
        }
    }
}
