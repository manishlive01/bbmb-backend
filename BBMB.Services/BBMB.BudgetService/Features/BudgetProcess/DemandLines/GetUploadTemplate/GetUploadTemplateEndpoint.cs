using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetUploadTemplate
{
    public sealed class GetUploadTemplateEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // F07.Enter: the right to frame the works demand; the key is checked in the handler (design §8.1).
            app.MapGet("/api/budget-proposals/{id:long}/upload-template",
                    async (long id, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetUploadTemplateQuery(id), ct),
                            r => Results.File(r.Data!.Content, r.Data.ContentType, r.Data.FileName)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetWorksDemandUploadTemplate")
                .WithTags("BudgetDemandLines")
                .Produces(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
