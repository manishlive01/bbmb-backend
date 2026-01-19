using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.Delete
{
    public sealed record DeleteMinorHeadCommand(string MinorAccCode) : IRequest<Response>;
}
