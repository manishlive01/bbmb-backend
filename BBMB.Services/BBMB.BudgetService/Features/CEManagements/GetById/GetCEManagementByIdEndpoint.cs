using BBMB.BudgetService.Features.Budgets.GetById;
using BBMB.BudgetService.Features.CEManagements.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.CEManagements.GetById
{
    public class GetCEManagementByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/ce-management/{ceCode}",
                async (int ceCode, IMediator mediator) =>
                {
                    if (ceCode <= 0)
                    {
                        return Results.BadRequest(new Response
                        {
                            IsSucced = false,
                            Message = "CE Code is required"
                        });
                    }

                    var query = new GetCEManagementByIdQuery(ceCode);
                    var response = await mediator.Send(query);

                    if (!response.IsSucced)
                        return Results.NotFound(response);

                    return Results.Ok(response);
                })
                .WithName("GetCEManagementById")
                .WithTags("CEManagement")
                .WithSummary("Get CE Management record by CE Code")
                .WithDescription("Retrieves CE Management details by CE Code")
                .Produces<Response<GetCEManagementByIdResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status400BadRequest);

        }
    }
}
