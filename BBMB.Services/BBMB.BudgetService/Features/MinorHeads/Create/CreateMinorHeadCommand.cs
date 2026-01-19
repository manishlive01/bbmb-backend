using BBMB.BudgetService.Features.MinorHeads.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.Create
{
    public sealed record CreateMinorHeadCommand(CreateMinorHeadDto dto) : IRequest<Response>;
}
