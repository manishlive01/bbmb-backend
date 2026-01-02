using BBMB_Budget.Features.Geographical.Districts.Update.Dtos;
using BBMB_Budget.Features.Geographical.Districts.Entities;
using Mapster;

namespace BBMB_Budget.Features.Geographical.Districts.Update
{
    public static class UpdateDistrictMasterMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<UpdateDistrictMasterDto, DistrictMaster>
                .NewConfig()

                // ===== Allowed updates =====
                .Map(dest => dest.StateMasterId,
                  src => src.StateMasterId)

                .Map(dest => dest.DistrictCode,
                     src => src.DistrictCode.Trim().ToUpper())

                .Map(dest => dest.DistrictName,
                     src => src.DistrictName.Trim())

                .Map(dest => dest.IsActive,
                     src => src.IsActive)

                // ===== Never update these =====
                .Ignore(dest => dest.DistrictMasterId)
                .Ignore(dest => dest.IsDeleted)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.DeletedAt);
        }
    }
}
