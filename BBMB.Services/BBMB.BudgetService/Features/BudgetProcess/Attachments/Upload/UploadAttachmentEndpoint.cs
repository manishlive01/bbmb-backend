using BBMB.BudgetService.Features.BudgetProcess.Attachments.Upload.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BBMB.BudgetService.Features.BudgetProcess.Attachments.Upload
{
    public sealed class UploadAttachmentEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Attachment.Upload: roles holding an Enter or Act right (design §4.4); view-only roles cannot fill storage.
            app.MapPost("/api/budget-attachments",
                    async ([FromForm] IFormFile file, [FromForm] string evidenceClass, IMediator mediator, CancellationToken ct) =>
                    {
                        var response = await mediator.Send(new UploadAttachmentCommand(file, evidenceClass), ct);
                        if (response.ScannerUnavailable)
                            return Results.Json((object)response, statusCode: StatusCodes.Status503ServiceUnavailable);
                        return BudgetResults.From(response,
                            r => Results.Created($"/api/budget-attachments/{r.Data!.AttachmentId}", r));
                    })
                .RequireAuthorization("BudgetRight:Attachment.Upload")
                .DisableAntiforgery()
                .Accepts<IFormFile>("multipart/form-data")
                .WithName("UploadBudgetAttachment")
                .WithTags("BudgetAttachments")
                .Produces<Response<UploadAttachmentResponseDto>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<Response>(StatusCodes.Status503ServiceUnavailable)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
