using BBMB.BudgetService.Features.Offices.RemittanceOffices.Create.Dtos;
using BBMB.BudgetService.Features.Pools.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Pools.Create
{
    public sealed record CreatePoolCommand(CreatePoolDto dto) : IRequest<Response>;
}
