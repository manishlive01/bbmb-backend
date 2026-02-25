using BBMB.BudgetService.Features.CEManagements.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.CEManagements.Update
{
    public class UpdateCEManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/ce-management/{ceCode}",
                    async (
                        string ceCode,
                        UpdateCEManagementDto dto,
                        IMediator mediator) =>
                    {
                        if (string.IsNullOrWhiteSpace(ceCode))
                        {
                            return Results.BadRequest(new Response
                            {
                                IsSucced = false,
                                Message = "CE Code is required"
                            });
                        }

                        var command = new UpdateCEManagementCommand(ceCode, dto);
                        var response = await mediator.Send(command);

                        if (!response.IsSucced)
                            return Results.NotFound(response);

                        return Results.Ok(response);
                    })
                    .WithName("UpdateCEManagement")
                    .WithTags("CEManagement")
                    .WithSummary("Update CE Management record")
                    .WithDescription("Updates an existing CE Management record by CE Code")
                    .Produces<Response>(StatusCodes.Status200OK)
                    .Produces<Response>(StatusCodes.Status404NotFound)
                    .Produces<Response>(StatusCodes.Status400BadRequest);
        }
    }
}
