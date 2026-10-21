using BBMB.BudgetService.Features.Works.SetLegacyCode.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.SetLegacyCode
{
    public sealed record SetLegacyCodeCommand(int Id, SetLegacyCodeDto dto) : IRequest<Response>;
}
