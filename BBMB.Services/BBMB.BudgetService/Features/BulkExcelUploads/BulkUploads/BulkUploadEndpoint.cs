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
                IMediator mediator) =>
            {
                if (string.IsNullOrWhiteSpace(type))
                {
                    return Results.BadRequest(new Response
                    {
                        IsSucced = false,
                        Message = "Type is required."
                    });
                }

                var command = new BulkUploadCommand(file, type);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            }).DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithName("BulkUploads")
            .WithTags("BulkUploads")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status400BadRequest);
        }
    }
}