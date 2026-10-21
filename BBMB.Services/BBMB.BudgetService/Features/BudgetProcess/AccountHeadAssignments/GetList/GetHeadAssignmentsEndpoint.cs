using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetList
{
    public sealed class GetHeadAssignmentsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/account-head-assignments",
                    async (IMediator mediator, CancellationToken ct, [FromQuery] long? officeId = null) =>
                        BudgetResults.Ok(await mediator.Send(new GetHeadAssignmentsQuery(officeId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetAccountHeadAssignments")
                .WithTags("AccountHeadAssignments")
                .Produces<Response<List<AssignedHeadDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
