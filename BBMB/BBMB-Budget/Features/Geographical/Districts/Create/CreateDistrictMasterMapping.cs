using BBMB_Budget.Features.Geographical.Districts.Create.Dtos;
using BBMB_Budget.Features.Geographical.Districts.Entities;
using Mapster;

namespace BBMB_Budget.Features.Geographical.Districts.Create
{
    public static class CreateDistrictMasterMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<CreateDistrictMasterDto, DistrictMaster>
                .NewConfig()

               // ===== Simple direct mappings =====
               .Map(dest => dest.StateMasterId,
                  src => src.StateMasterId)

                .Map(dest => dest.DistrictCode,
                     src => src.DistrictCode.Trim().ToUpper())

                .Map(dest => dest.DistrictName,
                     src => src.DistrictName.Trim())

                .Map(dest => dest.IsActive,
                     src => src.IsActive)

                // ===== Ignore DB / system-controlled fields =====
                .Ignore(dest => dest.DistrictMasterId)
                .Ignore(dest => dest.IsDeleted)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.DeletedAt);
        }
    }
}
