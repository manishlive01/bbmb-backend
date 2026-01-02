using BBMB_Budget.Features.Geographical.States.Entities;
using BBMB_Budget.Features.Geographical.States.GetList.Dtos;
using Mapster;

namespace BBMB_Budget.Features.Geographical.States.GetList
{
    public static class GetStateMasterListMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<StateMaster, GetStateMasterListResponseDto>
                .NewConfig()

                // ===== Simple direct mappings =====
                .Map(dest => dest.StateMasterId, src => src.StateMasterId)
                .Map(dest => dest.StateCode, src => src.StateCode)
                .Map(dest => dest.StateName, src => src.StateName)
                .Map(dest => dest.IsActive, src => src.IsActive);
        }
    }
}
