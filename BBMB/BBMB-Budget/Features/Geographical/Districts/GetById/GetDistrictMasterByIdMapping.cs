using BBMB_Budget.Features.Geographical.Districts.Entities;
using BBMB_Budget.Features.Geographical.Districts.GetById.Dtos;
using Mapster;

namespace BBMB_Budget.Features.Geographical.Districts.GetById
{
    public static class GetDistrictMasterByIdMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<DistrictMaster, GetDistrictMasterByIdResponseDto>
                .NewConfig()

                // ===== State fields =====
                .Map(dest => dest.StateMasterId,
                     src => src.StateMasterId)

                .Map(dest => dest.StateName,
                     src => src.StateMasters.StateName)

                // ===== District fields =====
                .Map(dest => dest.DistrictCode,
                     src => src.DistrictCode)

                .Map(dest => dest.DistrictName,
                     src => src.DistrictName)

                .Map(dest => dest.IsActive,
                     src => src.IsActive);
        }
    }
}
