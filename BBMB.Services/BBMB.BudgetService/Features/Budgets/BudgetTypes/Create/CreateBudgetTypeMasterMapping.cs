using BBMB.BudgetService.Features.Budgets.BudgetTypes.Create.Dtos;
using BBMB.BudgetService.Features.Budgets.BudgetTypes.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.Create
{
    public static class CreateBudgetTypeMasterMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<CreateBudgetTypeMasterDto, BudgetTypeMaster>
                .NewConfig()

                // ===== Simple direct mappings =====
                .Map(dest => dest.BudgetTypeCode,
                     src => src.BudgetTypeCode.Trim().ToUpper())

                .Map(dest => dest.BudgetTypeName,
                     src => src.BudgetTypeName.Trim())

                .Map(dest => dest.Description,
                     src => string.IsNullOrWhiteSpace(src.Description)
                            ? null
                            : src.Description.Trim())

                .Map(dest => dest.IsActive,
                     src => src.IsActive)

                // ===== Ignore DB / system-controlled fields =====
                .Ignore(dest => dest.BudgetTypeMasterId)
                .Ignore(dest => dest.IsDeleted)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.DeletedAt)
                .Ignore(dest => dest.BudgetMasters);
        }
    }
}
