using BBMB.BudgetService.Features.CRManagements.Create.Dtos;
using BBMB.BudgetService.Features.CRManagements.Entities;
using Mapster;

namespace BBMB.BudgetService.Features.CRManagements.Create
{
    public class CreateCRManagementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateCRManagementDto, CRManagement>()
                .Map(dest => dest.CEId, src => src.CEId)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)

                // 🔒 Backend controlled fields (never from client)
                .Ignore(dest => dest.Date)
                .Ignore(dest => dest.UserCode)

                // 🔒 Identity field (auto generated)
                .Ignore(dest => dest.Id)

                // 🔒 Navigation property must not be mapped
                .Ignore(dest => dest.CEManagement);
        }
    }
}
