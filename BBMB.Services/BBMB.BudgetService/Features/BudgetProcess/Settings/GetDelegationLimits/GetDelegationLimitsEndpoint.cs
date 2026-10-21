using BBMB.BudgetService.Features.BudgetProcess.Settings.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BBMB.BudgetService.Features.BudgetProcess.Settings.GetDelegationLimits
{
    public sealed class GetDelegationLimitsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-settings/delegation-limits",
                    async (IMediator mediator, CancellationToken ct, [FromQuery] DateOnly? on = null) =>
                        BudgetResults.Ok(await mediator.Send(new GetDelegationLimitsQuery(on), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetDelegationLimits")
                .WithTags("BudgetSettings")
                .Produces<Response<IReadOnlyList<DelegationLimitDto>>>(StatusCodes.Status200OK);
        }
    }
}
