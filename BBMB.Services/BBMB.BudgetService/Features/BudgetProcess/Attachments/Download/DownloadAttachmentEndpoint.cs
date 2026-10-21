using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Attachments.Download
{
    public sealed class DownloadAttachmentEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-attachments/{id:long}",
                    async (long id, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new DownloadAttachmentQuery(id), ct),
                            r => Results.File(r.Data!.Content, r.Data.ContentType, r.Data.FileName)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("DownloadBudgetAttachment")
                .WithTags("BudgetAttachments")
                .Produces(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
