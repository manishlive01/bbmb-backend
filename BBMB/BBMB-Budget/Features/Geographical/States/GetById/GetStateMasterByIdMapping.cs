using BBMB_Budget.Features.Geographical.States.Entities;
using BBMB_Budget.Features.Geographical.States.GetById.Dtos;
using Mapster;

namespace BBMB_Budget.Features.Geographical.States.GetById
{
    public static class GetStateMasterByIdMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<StateMaster, GetStateMasterByIdResponseDto>
                .NewConfig()

                // ===== Simple direct mappings =====
                .Map(dest => dest.StateCode, src => src.StateCode)
                .Map(dest => dest.StateName, src => src.StateName)
                .Map(dest => dest.IsActive, src => src.IsActive);
        }
    }
}
