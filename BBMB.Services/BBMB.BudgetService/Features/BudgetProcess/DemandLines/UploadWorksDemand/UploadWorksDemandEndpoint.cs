using BBMB.BudgetService.Features.BudgetProcess.DemandLines.UploadWorksDemand.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.UploadWorksDemand
{
    public sealed class UploadWorksDemandEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // F07.Enter: the right to frame the works demand; the key is checked in the handler (design §8.1).
            app.MapPost("/api/budget-proposals/{id:long}/upload",
                    async (long id, [FromForm] IFormFile file, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new UploadWorksDemandCommand(id, file), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .DisableAntiforgery()
                .Accepts<IFormFile>("multipart/form-data")
                .WithName("UploadWorksDemand")
                .WithTags("BudgetDemandLines")
                .Produces<Response<UploadResultDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<RuleFailuresDto>>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
