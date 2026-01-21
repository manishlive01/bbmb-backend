using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.Delete
{
    public record DeleteSubMinorHeadCommand(string SubMinorAccCode) : IRequest<Response>;
}
