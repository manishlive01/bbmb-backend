using BBMB_Budget.Features.Geographical.Districts.Entities;
using BBMB_Budget.Features.Geographical.Districts.GetList.Dtos;
using Mapster;

namespace BBMB_Budget.Features.Geographical.Districts.GetList
{
    public static class GetDistrictMasterListMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<DistrictMaster, GetDistrictMasterListResponseDto>
                .NewConfig()

                // ===== State fields =====
                .Map(dest => dest.StateMasterId,
                     src => src.StateMasterId)

                .Map(dest => dest.StateName,
                     src => src.StateMaster.StateName)

                 // ===== District fields =====
                 .Map(dest => dest.DistrictMasterId,
                     src => src.DistrictMasterId)

                .Map(dest => dest.DistrictCode,
                     src => src.DistrictCode)

                .Map(dest => dest.DistrictName,
                     src => src.DistrictName)

                .Map(dest => dest.IsActive,
                     src => src.IsActive);
        }
    }
}
