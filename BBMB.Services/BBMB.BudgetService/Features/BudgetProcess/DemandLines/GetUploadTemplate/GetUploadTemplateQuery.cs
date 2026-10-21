using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetUploadTemplate.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetUploadTemplate
{
    /// <summary>
    /// Design §8.5 (<c>GET /api/budget-proposals/{id}/upload-template</c>, BUD-097): the Excel template the
    /// module accepts for the works demand, carrying the F-07 columns in order.
    /// </summary>
    public sealed record GetUploadTemplateQuery(long ProposalId) : IRequest<Response<UploadTemplateFileDto>>;
}
