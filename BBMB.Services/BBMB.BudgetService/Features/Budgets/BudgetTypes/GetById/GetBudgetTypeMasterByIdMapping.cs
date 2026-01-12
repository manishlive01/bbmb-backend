using BBMB.BudgetService.Features.Budgets.BudgetTypes.Entities;
using BBMB.BudgetService.Features.Budgets.BudgetTypes.GetById.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.GetById
{
    public static class GetBudgetTypeMasterByIdMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<BudgetTypeMaster, GetBudgetTypeMasterByIdResponseDto>
                .NewConfig()

                // ===== Simple direct mappings =====
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
