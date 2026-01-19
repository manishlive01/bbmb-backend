using BBMB.BudgetService.Features.Classes.Entities;
using BBMB.BudgetService.Features.Classes.Update.Dtos;
using Mapster;

namespace BBMB.BudgetService.Features.Classes.Update
{
    public class UpdateClassMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateClassDto, Class>()
                .Map(dest => dest.ClassDesc, src => src.ClassDesc)
                .Ignore(dest => dest.ClassCode)
                .Ignore(dest => dest.ModificationDate);
        }
    }
}
