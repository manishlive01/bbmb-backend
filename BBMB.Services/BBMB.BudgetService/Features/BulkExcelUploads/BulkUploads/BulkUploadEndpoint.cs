using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.BulkUploads
{
    public class BulkUploadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/bulk-upload",
            async (
                [FromForm] IFormFile file,
                [FromForm] string type,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new Response
                    {
                        IsSucced = false,
                        Message = "File is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(type))
                {
                    return Results.BadRequest(new Response
                    {
                        IsSucced = false,
                        Message = "UploadType is required."
                    });
                }

                var command = new BulkUploadCommand(file, type);
                var response = await mediator.Send(command, cancellationToken);

                // Catalogued failures (M-20 → 403, "Row n: M-nn" → 409) go through BudgetResults (design §4.9);
                // the sheet-validation failures that carry no message id keep their existing 400.
                if (!response.IsSucced && response.MessageId is null)
                    return Results.BadRequest(response);
                return BudgetResults.From(response, r => Results.Ok(r));
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithName("BulkUploads")
            .WithTags("BulkUploads")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .Produces<Response>(StatusCodes.Status403Forbidden)
            .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}