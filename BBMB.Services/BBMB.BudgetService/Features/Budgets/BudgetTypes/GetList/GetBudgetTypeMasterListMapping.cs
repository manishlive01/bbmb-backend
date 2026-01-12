using BBMB.BudgetService.Features.Budgets.BudgetTypes.Entities;
using BBMB.BudgetService.Features.Budgets.BudgetTypes.GetList.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.GetList
{
    public static class GetBudgetTypeMasterListMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<BudgetTypeMaster, GetBudgetTypeMasterListResponseDto>
                .NewConfig()

                // ===== Simple direct mappings =====
                .Map(dest => dest.BudgetTypeMasterId,
                     src => src.BudgetTypeMasterId)

                .Map(dest => dest.BudgetTypeCode,
                     src => src.BudgetTypeCode)

                .Map(dest => dest.BudgetTypeName,
                     src => src.BudgetTypeName)

                .Map(dest => dest.Description,
                     src => src.Description)

                .Map(dest => dest.IsActive,
                     src => src.IsActive);
        }
    }
}
