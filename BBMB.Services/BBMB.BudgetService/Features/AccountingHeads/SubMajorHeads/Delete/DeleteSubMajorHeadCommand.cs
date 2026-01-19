using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Delete
{
    public sealed record DeleteSubMajorHeadCommand(string SubMajorAccCode) : IRequest<Response>;
}
