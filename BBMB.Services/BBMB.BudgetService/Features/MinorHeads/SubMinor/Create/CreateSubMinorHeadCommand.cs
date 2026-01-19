using BBMB.BudgetService.Features.MinorHeads.SubMinor.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.SubMinor.Create
{
    public record CreateSubMinorHeadCommand(CreateSubMinorHeadDto dto) : IRequest<Response>;
}
