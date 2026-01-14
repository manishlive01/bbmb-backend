using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Create.Dtos;
using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Create
{
    public class CreateMajorHeadMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateMajorHeadDto, MajorHead>()
                .Map(dest => dest.MajorAccCode, src => src.MajorAccCode)
                .Map(dest => dest.MajorAccDesc, src => src.MajorAccDesc)
                .Map(dest => dest.UserCode, src => src.UserCode)
                // 🔒 Ignore client-provided ModificationDate
                .Ignore(dest => dest.ModificationDate)
                // 🔒 Navigation properties must never be mapped from DTO
                .Ignore(dest => dest.SubMajorHeads);
        }
    }
}
