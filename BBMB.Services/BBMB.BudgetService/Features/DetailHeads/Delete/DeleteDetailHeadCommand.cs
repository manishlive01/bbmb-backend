using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.DetailHeads.Delete
{
    public sealed record DeleteDetailHeadCommand(string DetailAccCode) : IRequest<Response>;
}
