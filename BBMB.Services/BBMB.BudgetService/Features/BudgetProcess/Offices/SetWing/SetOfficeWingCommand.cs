using BBMB.BudgetService.Features.BudgetProcess.Offices.SetWing.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.SetWing
{
    public sealed record SetOfficeWingCommand(long OfficeId, SetOfficeWingDto Dto) : IRequest<Response>;
}
